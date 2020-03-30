GO
CREATE TABLE country(
alpha3code CHAR(3) PRIMARY KEY,
name VARCHAR,
alpha2code CHAR(2) NULL,
capital VARCHAR NULL,
region VARCHAR NULL,
subregion VARCHAR NULL,
population INT,
denonym VARCHAR NULL,
area FLOAT NULL,
giniIndex FLOAT NULL,
nativeName VARCHAR NULL,
numericCode VARCHAR NULL,
flag VARCHAR NULL,
cioc CHAR(3) NULL,
latlong VARCHAR NULL,
borders VARCHAR NULL,
topLevelDomain VARCHAR NULL,
callingCodes VARCHAR NULL,
timezones VARCHAR NULL,
altSpellings VARCHAR NULL
)

GO
CREATE TABLE currency(
code CHAR(3) PRIMARY KEY,
name VARCHAR NULL,
symbol CHAR(5) NULL,
alpha3Code CHAR(3) REFERENCES country(alpha3Code),
)

GO
CREATE TABLE language(
iso639_2 CHAR(3) PRIMARY KEY,
iso639_1 CHAR(2) NULL,
name VARCHAR NULL,
nativeName VARCHAR,
alpha3Code CHAR(3) REFERENCES country(alpha3Code),
)

GO
CREATE TABLE translations(
alpha3Code CHAR(3) PRIMARY KEY REFERENCES country(alpha3Code),
de VARCHAR(100),
es VARCHAR(100),
fr VARCHAR(100),
ja VARCHAR(100),
it VARCHAR(100),
br VARCHAR(100),
pt VARCHAR(100),
nl VARCHAR(100),
hr VARCHAR(100),
fa VARCHAR(100)
)

GO
CREATE TABLE regionalBloc(
acronym VARCHAR(10) PRIMARY KEY,
name VARCHAR(100),
otherAcronyms VARCHAR,
otherNames VARCHAR,
alpha3Code CHAR(3) REFERENCES country(alpha3Code)
)

go
