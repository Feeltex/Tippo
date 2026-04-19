create table rolle (
rollenID int primary key,
bezeichnung varchar(50)
);

create table benutzer (
benutzerID int generated always as identity primary key,
benutzername varchar(50) not null unique,
email varchar(150) not null unique,
passwort_hash varchar(200) not null,
erstellungsdatum timestamp not null,
rollenID int not null,
foreign key (rollenID) references rolle(rollenID)
);

create table mannschaft (
mannschaftsID int primary key,
name varchar(200) not null
);

create table spiel (
spielID integer generated always as identity primary key,
wettbewerb varchar(100) not null,
saison varchar (50) not null,
spieltag int not null,
anstosszeit timestamp not null,
ergebnisHeim int,
ergebnisGast int,
mannschaftHeimID int not null,
mannschaftGastID int not null,
foreign key (mannschaftHeimID) references mannschaft(mannschaftsID),
foreign key (mannschaftGastID) references mannschaft(mannschaftsID)
);

create table tipp (
tippID int generated always as identity primary key,
tippHeim int not null,
tippGast int not null,
benutzerID int not null,
spielID int not null,
foreign key (spielID) references spiel(spielID),
foreign key (benutzerID) references benutzer(benutzerID)
);