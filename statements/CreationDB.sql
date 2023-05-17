CREATE TABLE IF NOT EXISTS Employees (
    idEmployee VARCHAR(7) NOT NULL,
    surname VARCHAR(45) NOT NULL,
    Name_ VARCHAR(45) NOT NULL,
    birth DATE NOT NULL,
    phoneNumber INT NOT NULL,
    bankCard INT NOT NULL,
    PRIMARY KEY (idEmployee),
    INDEX (surname)
);

CREATE TABLE IF NOT EXISTS Admins(
    idAdmin INT UNSIGNED NOT NULL AUTO_INCREMENT,
    idEmployee VARCHAR(7) NOT NULL,
    monthSalary DECIMAL(10, 2),
    PRIMARY KEY(idAdmin),
    FOREIGN KEY fk_adminEmployee(idEmployee) REFERENCES Employees(idEmployee)
);

CREATE TABLE IF NOT EXISTS Couriers(
    idCourier INT UNSIGNED NOT NULL AUTO_INCREMENT,
    idEmployee VARCHAR(7) NOT NULL,
    courierAdminId INT UNSIGNED NOT NULL,
    PRIMARY KEY(idCourier),
    FOREIGN KEY fk_courierEmployee(idEmployee) REFERENCES Employees(idEmployee),
    FOREIGN KEY fk_couriersAdmin(courierAdminId) REFERENCES Admins(idAdmin)
);

CREATE TABLE IF NOT EXISTS Passwords(
    idPassOrder INT UNSIGNED NOT NULL AUTO_INCREMENT,
    idEmployee VARCHAR(7) NOT NULL UNIQUE,
    login_ VARCHAR(45) UNIQUE,
    password_ VARCHAR(45) UNIQUE,
    adminCheck TINYINT,
    PRIMARY KEY (idPassOrder),
    FOREIGN KEY fk_passEmployee(idEmployee) REFERENCES Employees(idEmployee)
);

CREATE TABLE IF NOT EXISTS Orders(
    idOrder INT UNSIGNED NOT NULL AUTO_INCREMENT,
    idCourier INT UNSIGNED DEFAULT NULL,
    deliveryStart DATETIME NOT NULL,
    #example: 50.042758532664884, 36.28471029653125
    destinationCoord VARCHAR(50),
    deliveryEnd DATETIME DEFAULT NULL,
    deliveryCost DECIMAL(10, 2),
    clientPhone VARCHAR (45),
    clientName VARCHAR (45),
    deliveryActuality TINYINT DEFAULT '1',
    orderName VARCHAR(45) NOT NULL,
    orderDescription VARCHAR(500) DEFAULT NULL,
    PRIMARY KEY(idOrder),
    FOREIGN KEY fk_orderCourier(idCourier) REFERENCES Couriers(idCourier),
    INDEX (clientPhone),
    INDEX(deliveryActuality)
);

CREATE VIEW CourierView AS
    SELECT
    `idOrder`,`deliveryActuality`, `destinationCoord`, `deliveryCost`, `clientPhone`, `clientName`, `orderName`, `orderDescription`
    FROM `Orders`
    WHERE `deliveryActuality`=1;