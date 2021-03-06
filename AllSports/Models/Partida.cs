﻿using AllSports.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AllSports.Models
{
    public class Partida
    {
        public int Id { get; set; }
        private Campeonato Campeonato { get; set; }
        public Time TimeCasa { get; set; }
        public Time TimeVisitante { get; set; }
        public int GolCasa { get; set; }
        public int GolVisitante { get; set; }
        public string Endereco { get; set; }
        public DateTime Data { get; set; }
        public bool Finalizada { get; set; }
        public int NumPartida { get; set; }

        public Partida(int id, Campeonato campeonato, Time timeCasa, Time timeVisitante, int golCasa, int golVisitante, string endereco,
            DateTime data, bool finalizada, int numPartida)
        {
            Id = id;
            Campeonato = campeonato;
            TimeCasa = timeCasa;
            TimeVisitante = timeVisitante;
            GolCasa = golCasa;
            GolVisitante = golVisitante;
            Endereco = endereco;
            Data = data;
            Finalizada = finalizada;
            NumPartida = numPartida;
        }

        private static void Validar(Campeonato campeonato, Time timeCasa, Time timeVisitante, ref int golCasa,
            ref int golVisitante, ref string endereco, ref DateTime data)
        {
            if (campeonato == null)
                throw new ValidationException("Campeonato inexistente");

            if (string.IsNullOrWhiteSpace(endereco))
                throw new ValidationException("Endereço vazio");

            endereco = endereco.Trim();
            if (endereco.Length > 52)
                throw new ValidationException("Nome muito longo");

            if (data < new DateTime(1900, 1, 1))
                throw new ValidationException("data_inicio muito antiga");

            if (data > DateTime.Now.AddYears(1))
                throw new ValidationException("data_inicio muito futurista");

        }

        private static Partida ObterPorId(int id, SqlConnection conn)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT id_campeonato, id_time_casa, id_time_visitante, gol_casa, gol_visitante,"
                   + "endereco, data, finalizada, partida FROM tbPartida WHERE id=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read() == false)
                    {
                        return null;
                    }
                    return new Partida(id, Campeonato.ObterPorId(reader.GetInt32(0), conn), Time.ObterPorId(reader.GetInt32(1)),
                        Time.ObterPorId(reader.GetInt32(2)), reader.GetInt32(3), reader.GetInt32(4), reader.GetString(5),
                        reader.GetDateTime(6), reader.GetBoolean(7), reader.GetInt32(8));
                }
            }
        }

        public static List<Partida> ObterPorCampeonato(int id)
        {
            using (SqlConnection conn = Sql.Open())
            {
                Time time_casa = null, time_visitante = null;
                Campeonato campeonato = Campeonato.ObterPorId(id, conn);
                List<Time> times = Time.ObterPorCampeonato(id);

                using (SqlCommand cmd = new SqlCommand(@"
					select id, id_campeonato, id_time_casa, id_time_visitante, gol_casa, gol_visitante, endereco, data, finalizada, partida
					from tbPartida
					where id_campeonato = @id order by partida", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<Partida> partidas = new List<Partida>();

                        while (reader.Read() == true)
                        {
                            for (int i = 0; i < times.Count; i++)
                            {
                                try
                                {
                                    if (times[i].Id == reader.GetInt32(2))
                                    {
                                        time_casa = times[i];
                                    }
                                }
                                catch (Exception e)
                                {

                                }

                                try
                                {
                                    if (times[i].Id == reader.GetInt32(3))
                                    {
                                        time_visitante = times[i];
                                    }
                                }
                                catch (Exception ex)
                                {

                                }

                            }

                            partidas.Add(new Partida(reader.GetInt32(0), campeonato, time_casa, time_visitante, reader.GetInt32(4), reader.GetInt32(5), reader.GetString(6), reader.GetDateTime(7), reader.GetBoolean(8), reader.GetInt32(9)));
                            time_casa = null;
                            time_visitante = null;
                        }

                        return partidas;
                    }
                }
            }

        }


        public static Partida Criar(Campeonato campeonato, Time timeCasa, Time timeVisitante,
                                    int golCasa, int golVisitante, string endereco, DateTime data, bool finalizada, int partida)
        {
            Validar(campeonato, timeCasa, timeVisitante, ref golCasa, ref golVisitante, ref endereco, ref data);

            using (SqlConnection conn = Sql.Open())
            {
                using (SqlCommand cmd = new SqlCommand("INSERT INTO tbPartida (id_campeonato, id_time_casa, id_time_visitante, gol_casa,"
                    + " gol_visitante, endereco, data, finalizada, partida) OUTPUT INSERTED.id VALUES (@id_campeonato, @id_time_casa, @id_time_visitante,"
                    + "@data_fim)", conn))
                {
                    cmd.Parameters.AddWithValue("@id_campeonato", campeonato.Id);
                    cmd.Parameters.AddWithValue("@id_time_casa", timeCasa.Id);
                    cmd.Parameters.AddWithValue("@id_time_visitante", timeVisitante.Id);
                    cmd.Parameters.AddWithValue("@gol_casa", golCasa);
                    cmd.Parameters.AddWithValue("@gol_visitante", golVisitante);
                    cmd.Parameters.AddWithValue("@endereco", endereco);
                    cmd.Parameters.AddWithValue("@data", data);
                    cmd.Parameters.AddWithValue("@partida", partida);

                    int id = (int)cmd.ExecuteScalar();

                    return new Partida(id, campeonato, timeCasa, timeVisitante, golCasa, golVisitante, endereco, data, finalizada, partida);
                }
            }
        }

        public static void SortearPartidas(int id, int[] id_times)
        {
            Random rnd = new Random();

            for (int i = 0; i < id_times.Length; i++)
            {
                int a = rnd.Next(id_times.Length);
                int temp = id_times[i];
                id_times[i] = id_times[a];
                id_times[a] = temp;
            }

            int contador = 1;

            DeletarTodasPartidasPorCampeonato(id);

            using (SqlConnection conn = Sql.Open())
            {
                if (id_times.Length > 0)
                {
                    for (int i = 0; i < id_times.Length; i++)
                    {
                        using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO tbPartida (id_campeonato, id_time_casa, id_time_visitante, gol_casa,
                        gol_visitante, endereco, data, finalizada, partida) VALUES (@id_campeonato, @id_time_casa, @id_time_visitante, @gol_casa,
                        @gol_visitante, @endereco, @data, @finalizada, @partida)", conn))
                        {
                            cmd.Parameters.AddWithValue("@id_campeonato", id);
                            cmd.Parameters.AddWithValue("@id_time_casa", id_times[i]);
                            cmd.Parameters.AddWithValue("@id_time_visitante", id_times[++i]);
                            cmd.Parameters.AddWithValue("@gol_casa", 0);
                            cmd.Parameters.AddWithValue("@gol_visitante", 0);
                            cmd.Parameters.AddWithValue("@endereco", "");
                            cmd.Parameters.AddWithValue("@data", DateTime.Now);
                            cmd.Parameters.AddWithValue("@finalizada", false);
                            cmd.Parameters.AddWithValue("@partida", contador++);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO tbPartida (id_campeonato, gol_casa,
                        gol_visitante, endereco, data, finalizada, partida) VALUES (@id_campeonato, @gol_casa, @gol_visitante, @endereco, @data, @finalizada, @partida)", conn))
                        {
                            cmd.Parameters.AddWithValue("@id_campeonato", id);
                            cmd.Parameters.AddWithValue("@gol_casa", 0);
                            cmd.Parameters.AddWithValue("@gol_visitante", 0);
                            cmd.Parameters.AddWithValue("@endereco", "");
                            cmd.Parameters.AddWithValue("@data", DateTime.Now);
                            cmd.Parameters.AddWithValue("@finalizada", false);
                            cmd.Parameters.AddWithValue("@partida", contador++);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public static void EditarPartidaPorID(int id, int gol_casa, int gol_visitante, string endereco, DateTime data, bool finalizada)
        {
            EditarPartidaPorID(id, gol_casa, gol_visitante, endereco, data, finalizada, null, null);
        }

        public static void EditarPartidaPorID(int id, int gol_casa, int gol_visitante, string endereco, DateTime data, bool finalizada, int? id_time_casa, int? id_time_visitante)
        {
            using (SqlConnection conn = Sql.Open())
            {
                using (SqlCommand cmd = new SqlCommand(@"
					update tbPartida 
					set 
					gol_casa = @gol_casa,
					gol_visitante = @gol_visitante,
					endereco = @endereco,
					data = @data,
					finalizada = @finalizada
					where id = @id
					", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@gol_casa", gol_casa);
                    cmd.Parameters.AddWithValue("@gol_visitante", gol_visitante);
                    cmd.Parameters.AddWithValue("@endereco", endereco);
                    cmd.Parameters.AddWithValue("@data", data);
                    cmd.Parameters.AddWithValue("@finalizada", finalizada);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void EditarPartidaProximaFasePorID(int id, int gol_casa, int gol_visitante, string endereco, DateTime data, bool finalizada, int id_time_casa, int id_time_visitante)
        {
            string comandoCasa = @"
					update tbPartida 
					set 
					gol_casa = @gol_casa,
					gol_visitante = @gol_visitante,
					endereco = @endereco,                    
					data = @data,
					finalizada = @finalizada,
                    id_time_casa = @id_time_casa             
					where id = @id
					";

            string comandoVisitante = @"
					update tbPartida 
					set 
					gol_casa = @gol_casa,
					gol_visitante = @gol_visitante,
					endereco = @endereco,                    
					data = @data,
					finalizada = @finalizada,
                    id_time_visitante = @id_time_visitante                
					where id = @id
					";

            using (SqlConnection conn = Sql.Open())
            {
                if (id_time_casa > 0)
                    using (SqlCommand cmd = new SqlCommand(comandoCasa, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@gol_casa", gol_casa);
                        cmd.Parameters.AddWithValue("@gol_visitante", gol_visitante);
                        cmd.Parameters.AddWithValue("@endereco", endereco);
                        cmd.Parameters.AddWithValue("@id_time_casa", id_time_casa);
                        cmd.Parameters.AddWithValue("@data", data);
                        cmd.Parameters.AddWithValue("@finalizada", finalizada);

                        cmd.ExecuteNonQuery();
                    }
                if (id_time_visitante > 0)
                    using (SqlCommand cmd = new SqlCommand(comandoVisitante, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@gol_casa", gol_casa);
                        cmd.Parameters.AddWithValue("@gol_visitante", gol_visitante);
                        cmd.Parameters.AddWithValue("@endereco", endereco);
                        cmd.Parameters.AddWithValue("@id_time_visitante", id_time_visitante);
                        cmd.Parameters.AddWithValue("@data", data);
                        cmd.Parameters.AddWithValue("@finalizada", finalizada);

                        cmd.ExecuteNonQuery();
                    }
            }
        }

        public static void ProximaPartida(int id_campeonato, int gols_casa, int gols_visitante, int? id_time_casa, int? id_time_visitante, int numeroPartida)
        {
            string comandoCasa = @"
                    INSERT INTO tbPartida 
                    (id_campeonato, id_time_casa, gol_casa, gol_visitante, endereco, data, finalizada, partida) 
                    VALUES 
                    (@id_campeonato, @id_time_casa, @gol_casa, @gol_visitante, @endereco, @data, @finalizada, @partida)";
            string comandoVisitante = @"
                    INSERT INTO tbPartida 
                    (id_campeonato, id_time_visitante, gol_casa, gol_visitante, endereco, data, finalizada, partida) 
                    VALUES 
                    (@id_campeonato, @id_time_visitante, @gol_casa, @gol_visitante, @endereco, @data, @finalizada, @partida)";
            string comandoNulos = @"
                    INSERT INTO tbPartida 
                    (id_campeonato, gol_casa, gol_visitante, endereco, data, finalizada, partida)
                    VALUES 
                    (@id_campeonato, @gol_casa, @gol_visitante, @endereco, @data, @finalizada, @partida)";

            using (SqlConnection conn = Sql.Open())
            {
                if (id_time_casa != null)
                {
                    using (SqlCommand cmd = new SqlCommand(comandoCasa, conn))
                    {
                        cmd.Parameters.AddWithValue("@id_campeonato", id_campeonato);
                        cmd.Parameters.AddWithValue("@id_time_casa", id_time_casa);
                        cmd.Parameters.AddWithValue("@gol_casa", gols_casa);
                        cmd.Parameters.AddWithValue("@gol_visitante", gols_visitante);
                        cmd.Parameters.AddWithValue("@endereco", "");
                        cmd.Parameters.AddWithValue("@data", DateTime.Now);
                        cmd.Parameters.AddWithValue("@finalizada", false);
                        cmd.Parameters.AddWithValue("@partida", numeroPartida);

                        cmd.ExecuteNonQuery();
                    }
                }

                if (id_time_visitante != null)
                {
                    using (SqlCommand cmd = new SqlCommand(comandoVisitante, conn))
                    {
                        cmd.Parameters.AddWithValue("@id_campeonato", id_campeonato);
                        cmd.Parameters.AddWithValue("@id_time_visitante", id_time_visitante);
                        cmd.Parameters.AddWithValue("@gol_casa", gols_casa);
                        cmd.Parameters.AddWithValue("@gol_visitante", gols_visitante);
                        cmd.Parameters.AddWithValue("@endereco", "");
                        cmd.Parameters.AddWithValue("@data", DateTime.Now);
                        cmd.Parameters.AddWithValue("@finalizada", false);
                        cmd.Parameters.AddWithValue("@partida", numeroPartida);

                        cmd.ExecuteNonQuery();
                    }
                }

                if ((id_time_casa == null || id_time_casa < 1) && (id_time_visitante == null || id_time_visitante < 1))
                {
                    using (SqlCommand cmd = new SqlCommand(comandoNulos, conn))
                    {
                        cmd.Parameters.AddWithValue("@id_campeonato", id_campeonato);
                        cmd.Parameters.AddWithValue("@gol_casa", gols_casa);
                        cmd.Parameters.AddWithValue("@gol_visitante", gols_visitante);
                        cmd.Parameters.AddWithValue("@endereco", "");
                        cmd.Parameters.AddWithValue("@data", DateTime.Now);
                        cmd.Parameters.AddWithValue("@finalizada", false);
                        cmd.Parameters.AddWithValue("@partida", numeroPartida);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void DeletarTodasPartidasPorCampeonato(int id_campeonato)
        {
            using (SqlConnection conn = Sql.Open())
            {
                using (SqlCommand cmd = new SqlCommand("DELETE from tbPartida where id_campeonato = @id_campeonato", conn))
                {
                    cmd.Parameters.AddWithValue("@id_campeonato", id_campeonato);
                    cmd.ExecuteNonQuery();
                }
            }

        }
    }
}