PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS User(
    UserId INTEGER PRIMARY KEY,
    Username TEXT,
    Password TEXT,
    Enabled INTEGER,
    UserAdmin INTEGER
);

CREATE TABLE IF NOT EXISTS User_Family(
    UserId INTEGER,
    FamilyName TEXT,
    Permissions TEXT,

    FOREIGN KEY(UserId) REFERENCES User(UserId) ON DELETE CASCADE
);

INSERT INTO User (
    Username,
    Password,
    Enabled,
    UserAdmin
)
SELECT  'test',
        '1FE3738FDFB62E06F206CE700CED4F91C42AADC64579C88315195E245938AC48F5ABF7423015967C3303B39A3D42A6F36F61647026061A5422827882E77D17B2',
        1,
        1
WHERE   NOT EXISTS (
    SELECT  1
    FROM    User
    WHERE   Username = 'test'
);

INSERT INTO User_Family (
    UserId,
    FamilyName,
    Permissions
)
SELECT  (SELECT UserId FROM User WHERE Username = 'test'),
        'TestFamily',
        '{ "Photos": { "Enabled": true } }'
WHERE   NOT EXISTS (
    SELECT  1
    FROM    User_Family
    WHERE   UserId = (SELECT UserId FROM User WHERE Username = 'test')
        AND FamilyName = 'TestFamily'
)