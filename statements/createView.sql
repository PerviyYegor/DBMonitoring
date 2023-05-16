CREATE VIEW CourierView AS
    SELECT
    `idOrder`,`deliveryActuality`, `destinationCoord`, `deliveryCost`, `clientPhone`, `clientName`, `orderName`, `orderDescription`
    FROM `Orders`
    WHERE `deliveryActuality`=1;