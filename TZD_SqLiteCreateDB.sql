
create table PRICE_DEALER
(
  code_dealer                 INTEGER not null, -- 'Цены по дилерским категориям';
  code_wares                  INTEGER not null, --'Код дилерской категории [spr.dealer.code_dealer]'
  price_dealer                NUMBER not null,
)
CREATE UNIQUE INDEX id_ADDITION_UNIT ON ADDITION_UNIT (code_dealer,CODE_WARES);

--Одиниці виміру
CREATE TABLE ADDITION_UNIT (
    CODE_WARES                   INTEGER NOT NULL,
    CODE_UNIT                    INTEGER NOT NULL,
    COEFFICIENT                  NUMBER  NOT NULL,
    DEFAULT_UNIT                 TEXT    NOT NULL
);
CREATE UNIQUE INDEX id_ADDITION_UNIT ON ADDITION_UNIT (CODE_WARES, CODE_UNIT);


-- Штрихкоди 
-- dbo.v_BAR_CODE_ADDITIONAL_UNIT
CREATE TABLE BAR_CODE_ADDITIONAL_UNIT (    
    CODE_WARES                    INTEGER NOT NULL,
    CODE_UNIT                     INTEGER NOT NULL,
    BAR_CODE                      TEXT    NOT NULL    
);

CREATE UNIQUE INDEX id_BAR_CODE_ADDITIONAL_UNIT ON BAR_CODE_ADDITIONAL_UNIT (CODE_WARES,CODE_UNIT,BAR_CODE);
CREATE INDEX ind_BAR_CODE_ADDITIONAL_UNIT ON BAR_CODE_ADDITIONAL_UNIT ( BAR_CODE);


--Товари
--dbo.v_WARES
CREATE TABLE WARES (
    CODE_WARES          INTEGER  NOT NULL,
    CODE_GROUP          INTEGER  NOT NULL,
    NAME_WARES          TEXT     NOT NULL,
    CODE_UNIT           INTEGER  NOT NULL,
    DESCRIPTION         TEXT,
    VAT                 NUMBER   NOT NULL,
    VAT_OPERATION       NUMBER   NOT NULL,
    NAME_WARES_RECEIPT  TEXT,
    DATE_INSERT         DATETIME NOT NULL,
    USER_INSERT         INTEGER  NOT NULL
);
CREATE UNIQUE INDEX id_WARES ON WARES ( CODE_WARES);
