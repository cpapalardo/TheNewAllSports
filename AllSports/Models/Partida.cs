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
        private int Id { get; set; }
        private Campeonato Campeonato { get; set; }
        private Time TimeCasa { get; set; }
        private Time TimeVisitante { get; set; }
        private int GolCasa { get; set; }
        private int GolVisitante { get; set; }
        private string Endereco { get; set; }
        private DateTime Data { get; set; }
        private bool Finalizada { get; set; }

        public Partida(int id, Campeonato campeonato, Time timeCasa, Time timeVisitante, int golCasa, int golVisitante, string endereco,
            DateTime data, bool finalizada)
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
                   + "endereco, data, finalizada FROM tbPartida WHERE id=@id", conn))
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
                        reader.GetDateTime(6), reader.GetBoolean(7));
                }
            }
        }

        public static List<Partida> ObterPorCampeonato(int id)
        {
            using (SqlConnection conn = Sql.Open())
            {
                Campeonato campeonato = Campeonato.ObterPorId(id, conn);

                using (SqlCommand cmd = new SqlCommand(
                    "select id, id_campeonato, id_time_casa, id_time_visitante, gol_casa, gol_visitante, endereco, data, finalizada " +
                    "from tbPartida where id_campeonato = @id order by id",
                    conn))
                {

                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<Partida> partidas = new List<Partida>();

                        while (reader.Read() == true)
                        {
                            partidas.Add(new Partida(reader.GetInt32(0), Campeonato.ObterPorId(reader.GetInt32(1)),Time.ObterPorId(reader.GetInt32(2)), 
                                Time.ObterPorId(reader.GetInt32(3)), reader.GetInt32(4), reader.GetInt32(5), reader.GetString(6),
                                reader.GetDateTime(7), reader.GetBoolean(8)));
                        }
                        return partidas;
                    }
                }
            }

        }


        public static Partida Criar(Campeonato campeonato, Time timeCasa, Time timeVisitante, 
                                    int golCasa, int golVisitante, string endereco, DateTime data, bool finalizada)
        {
            Validar(campeonato, timeCasa, timeVisitante, ref golCasa, ref golVisitante, ref endereco, ref data);

            using (SqlConnection conn = Sql.Open())
            {
                using (SqlCommand cmd = new SqlCommand("INSERT INTO tbPartida (id_campeonato, id_time_casa, id_time_visitante, gol_casa,"
                    + " gol_visitante, endereco, data, finalizada) OUTPUT INSERTED.id VALUES (@id_campeonato, @id_time_casa, @id_time_visitante,"
                    + "@data_fim)", conn))
                {
                    cmd.Parameters.AddWithValue("@id_campeonato", campeonato.Id);
                    cmd.Parameters.AddWithValue("@id_time_casa", timeCasa.Id);
                    cmd.Parameters.AddWithValue("@id_time_visitante", timeVisitante.Id);
                    cmd.Parameters.AddWithValue("@gol_casa", golCasa);
                    cmd.Parameters.AddWithValue("@gol_visitante", golVisitante);
                    cmd.Parameters.AddWithValue("@endereco", endereco);
                    cmd.Parameters.AddWithValue("@data", data);

                    int id = (int)cmd.ExecuteScalar();

                    return new Partida(id, campeonato, timeCasa, timeVisitante, golCasa, golVisitante, endereco, data, finalizada);
                }
            }
        }

    }
}