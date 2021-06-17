use Peerchat;

create table usermodes ( 
	id int auto_increment PRIMARY KEY,
	channelmask text NOT NULL,
	hostmask text NULL,
    comment text NULL,
    machineid text NULL,
    profileid int NULL,
    modeflags int NOT NULL DEFAULT 0,
    gameid int NULL,
    expiresAt DATETIME NULL,
    ircNick text NULL,
    setByHost text NULL,
    setByPid int NULL,
    setAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

create table Peerchat.chanprops (
    id int auto_increment PRIMARY KEY,
	channelmask text NOT NULL,
    password text NULL,
    entrymsg text NULL,
    comment text NULL,
    topic text NULL,
    expiresAt DATETIME NULL,
    groupname text NULL,
    `limit` int NULL,
    modeflags int NOT NULL DEFAULT 0,
    onlyOwner BIT NOT NULL DEFAULT 0,
    setByNick text NULL,
    setByPid INT NULL,
    setByHost text NULL,
    setAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);