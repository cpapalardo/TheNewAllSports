create table tbJogador(
	id int identity(1,1) not null,
	nome nvarchar(52) not null,
	apelido nvarchar (52) not null,
	email nvarchar (52) not null,
	senha nvarchar (52) not null
	
	constraint pk_jogador_id primary key clustered (id asc)
)

create table tbCampeonato(
	id int identity(1,1) not null,
	id_gerente int not null,
	nome nvarchar (52) not null,
	data_inicio datetime not null,
	data_fim datetime not null

	constraint fk_id_gerente foreign key (id_gerente) references tbJogador
	constraint pk_campeonato_id primary key clustered (id asc)
)


create table tbRodada(
	id int identity (1,1) not null,
	id_campeonato int not null,
	numero int not null

	constraint pk_rodada_id primary key clustered (id asc)
	constraint fk_campeonato_id foreign key (id_campeonato) references tbCampeonato
)

create table tbTime(
	id int identity (1,1) not null,
	id_coordenador int not null,
	nome nvarchar (52) not null,
	fundacao datetime not null

	constraint pk_time_id primary key clustered (id asc)
	constraint fk_coordenador_id foreign key (id_coordenador) references tbJogador
)

create table tbPartida(
	id int identity (1,1) not null,
	id_gerente int not null,
	id_time_casa int not null,
	id_time_visitante int not null,
	gol_casa int not null,
	gol_visitante int not null,
	endereco nvarchar(100) not null,
	data datetime not null,
	finalizada bit not null,

	constraint pk_partida_id primary key clustered (id asc),
	constraint fk_gerente__partida_id foreign key (id_gerente) references tbJogador,
	constraint fk_time_casa_id foreign key (id_time_casa) references tbTime,
	constraint fk_time_visitante_id foreign key (id_time_visitante) references tbTime
)

create table tbJogadorTime(
	id_jogador int not null,
	id_time int not null,

	constraint pk_jogadorTime primary key (id_jogador, id_time),
	constraint fk_jogador_id foreign key (id_jogador) references tbJogador(id),
	constraint fk_time_id foreign key (id_time) references tbTime(id)
)

create table tbPartidaRodada(
	id_partida int not null,
	id_rodada int not null,

	constraint pk_partidarodada primary key (id_partida, id_rodada),
	constraint fk_partida foreign key (id_partida) references tbPartida(id),
	constraint fk_rodada foreign key (id_rodada) references tbRodada (id)
)

drop table tbCampeonato
drop table tbJogador
drop table tbPartida
drop table tbRodada
drop table tbTime

