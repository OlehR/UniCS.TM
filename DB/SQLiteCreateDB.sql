-- Одиниці виміру
-- MSSQL dbo.v_addition_unit
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


-- Касові апарати
CREATE TABLE CASH_REGISTER (
    CODE_CASH_REGISTER          INTEGER  NOT NULL,
    SERIAL_NUMBER_CASH_REGISTER TEXT     NOT NULL,
    MODEL_CASH_REGISTER         TEXT     NOT NULL,
    DESCRIPTION                 TEXT,
    SIGN_ACTIVITY               TEXT     NOT NULL,
    FISCAL_NUMBER_CASH_REGISTER TEXT,
    DATE_BEGIN_CASH_REGISTER    DATETIME,
    DATE_END_CASH_REGISTER      DATETIME
);
CREATE INDEX idx_CASH_REGISTER ON CASH_REGISTER (CODE_CASH_REGISTER);

-- виборки даних(набори даних)
CREATE TABLE CHOICE_DATA (
    CODE_CHOICE INTEGER NOT NULL,
    CODE_DATA   INTEGER NOT NULL,
    CODE_DATA_2 INTEGER,
    CODE_ROW    INTEGER NOT NULL
);
CREATE UNIQUE INDEX id_choice_data ON CHOICE_DATA (CODE_CHOICE,CODE_DATA);


--!!!CITY

--Конфігурація
CREATE TABLE CONFIG (
    NAME_VAR    TEXT     NOT NULL,
    DATA_VAR    TEXT     NOT NULL,
    TYPE_VAR    TEXT     NOT NULL,
    DESCRIPTION TEXT,
    USER_CREATE INTEGER  NOT NULL,
    DATE_CREATE DATETIME NOT NULL
);

CREATE UNIQUE INDEX idx_CONFIG ON CONFIG ( NAME_VAR);

--!!! Дисконтрі програми --ИнформационныеКарты
--dbo.V_DISCOUNT_CARD
CREATE TABLE DISCOUNT_CARD (
    CODE_DISCOUNT_CARD           INTEGER NOT NULL,
    CODE_SUBGROUP                INTEGER NOT NULL,
    CODE_DISCOUNT_CARD_GROUP     INTEGER NOT NULL,
    NAME_DISCOUNT_CARD           TEXT    NOT NULL,
    PERCENT_DISCOUNT_CARD        NUMBER  NOT NULL,
    SUM_DISCOUNT_CARD            NUMBER  NOT NULL,
    CODE_CURRENCY                INTEGER NOT NULL,
    SIGN_ACTIVITY                TEXT    NOT NULL,
    FIXED_DISCOUNT               TEXT    NOT NULL,
    PERIOD_OF_VALIDITY           INTEGER,
    DESCRIPTION                  TEXT,
    TYPE_CALC_PERIOD_MIN_SUM     TEXT,
    SUM_RETENTION_MIN_SUM        NUMBER,
    MIN_REST_BONUS               NUMBER,
    IS_ALLOW_CHARGE_BONUS        TEXT,
    IS_ALLOW_WRITEOFF_BONUS      TEXT,
    PERIOD_GET_DISC_CARD         TEXT,
    PERIOD_HOLD_DISC_CARD        TEXT,
    COEF_CALC_BONUS              NUMBER  NOT NULL,
    SUM_BONUS_NOT_HOLD_DISC_CARD NUMBER  NOT NULL
);

CREATE UNIQUE INDEX id_DISCOUNT_CARD ON DISCOUNT_CARD ( CODE_DISCOUNT_CARD);

-- Фірми клієнти
-- dbo.v_CLIENT
CREATE TABLE CLIENT (
    CODE_CLIENT               INTEGER NOT NULL,
	NAME_CLIENT               TEXT,
    BAR_CODE                  TEXT,
    CODE_DISCOUNT_CARD        INTEGER
);

CREATE UNIQUE INDEX id_CLIENT ON CLIENT (CODE_CLIENT);
CREATE UNIQUE INDEX id_CLIENT_BAR ON CLIENT (BAR_CODE);


-- Інформація про поля для Грід
CREATE TABLE FIELD_INFO (
    NAME         TEXT     NOT NULL,
    TYPE_FILTER  INTEGER  NOT NULL,
    PRINT_NAME   TEXT     NOT NULL,
    EX_INFO      TEXT,
    DEFAULT_WITH INTEGER,
    MAX_WITH     INTEGER,
    MIN_WITH     INTEGER,
    DESCRIPTION  TEXT,
    DATE_CREATE  DATETIME NOT NULL,
    USER_CREATE  INTEGER  NOT NULL
);


 
--- Для генерації номерів докуметів(чеків
CREATE TABLE GEN_WORKPLACE (
    ID_WORKPLACE INTEGER NOT NULL,
    CODE_PERIOD  INTEGER NOT NULL,
    CODE_RECEIPT INTEGER NOT NULL
);
CREATE UNIQUE INDEX id_Gen_workplace ON GEN_WORKPLACE (ID_WORKPLACE,CODE_PERIOD);

-- Шаблони

CREATE TABLE PATTERNS (
    CODE_DOCUMENT  NUMBER NOT NULL,
    CODE_PATTERN   NUMBER NOT NULL,
    NAME_PATTERN   TEXT   NOT NULL,
    NIK_PATTERN    TEXT,
    --SIGN_ACTIVITY  TEXT   NOT NULL,
    STATE_DOCUMENT TEXT,
    DESCRIPTION    TEXT
);

CREATE UNIQUE INDEX id_patterns ON PATTERNS (CODE_PATTERN);


-- Ціни 
-- dbo.v_PRICE_DEALER
CREATE TABLE PRICE_DEALER (
    CODE_DEALER                 INTEGER  NOT NULL,
    CODE_WARES                  INTEGER  NOT NULL,
    PRICE_DEALER                NUMBER   NOT NULL
);
CREATE UNIQUE INDEX id_PRICE_DEALER ON PRICE_DEALER (CODE_DEALER,CODE_WARES);


-- Фізичні особи
CREATE TABLE PRIVAT (
    CODE_PRIVAT             INTEGER  NOT NULL,
    NAME_FOR_PRINT          TEXT     NOT NULL,
    BAR_CODE                TEXT,
    DESCRIPTION             TEXT
);
CREATE UNIQUE INDEX id_PRIVAT ON PRIVAT (CODE_PRIVAT);



-- Акції!!!!
--Акції (Шапка) MID i UnicS
CREATE TABLE PROMOTION_SALE (
    CODE_PS          INTEGER  NOT NULL,  --'Код'
    NAME_PS          TEXT     NOT NULL,  --'Назва'
    CODE_PARENT      INTEGER,            --'Шаблон'
    STATE            INTEGER  NOT NULL,  -- 'Стан (0-готується,1-підготовлено, 2 -діє,-1 - завершено)'
    DATE_BEGIN       DATETIME,           --'Дата початку акції'
    DATE_END         DATETIME,           --'Дата завершення акції'
    TYPE             INTEGER  NOT NULL,  --'Тип акції (1-На товари,2-на весь набір, 3-на товари в наборі )'
    TYPE_DATA        INTEGER  NOT NULL,  --'0-товари, 1-товари, 2-бренди, 3-групи товарів, 4-альтернативні товарні ієрархії, 5-альтернативні групові ієрархії, 6-категорії товарів ,7- конвертовані властивості товарів) (select t.*, t.rowid from C.DATA_NAME t where t.data_level=52)'
    PRIORITY         INTEGER  NOT NULL,  --'Пріоритет 1-мін'
    SUM_ORDER        NUMBER   NOT NULL,  --'Сума від якої діє акція'
    TYPE_WORK_COUPON INTEGER  NOT NULL,  --'0- без купона, 1 - на всі товари, 2 - тільки на товар зчитаний перед купоном.'
    BAR_CODE_COUPON  TEXT,               --'Штрихкод купона'
    DATE_CREATE      DATETIME NOT NULL,  --'Дата вставки\зміни акції'
    USER_CREATE      INTEGER  NOT NULL   --'Користувач вставки\зміни акції'
);
CREATE UNIQUE INDEX id_PROMOTION_SALE ON PROMOTION_SALE (CODE_PS);


--Акції Товарна частина MID i UnicS
CREATE TABLE PROMOTION_SALE_DATA (
    CODE_PS                   INTEGER  NOT NULL, --'Код акції'
    NUMBER_GROUP              INTEGER  NOT NULL, --'Номер групи'
    CODE_WARES                INTEGER  NOT NULL, --'Код товару, 0 - на всі товари згідно фільтрів. (тип знижки 1 заборонено)'
    USE_INDICATIVE            INTEGER  NOT NULL, --'0-не враховувати, 1-Враховувати.,2- враховувати і мінімальну'
    TYPE_DISCOUNT             INTEGER  NOT NULL, --'Тип знижки (1-ціна,2-знижка,3-%знижки, 4- заміна ДК) і тд (select t.*, t.rowid from C.DATA_NAME t where t.data_level=50)';
    ADDITIONAL_CONDITION      INTEGER  NOT NULL, --'0- на кожну позицію, 1-на кожну n- позицію, до n позиції,після n-кількості (n-data_ADDITIONAL_CONDITION )  (select t.*, t.rowid from C.DATA_NAME t where t.data_level=51)'
    DATA                      NUMBER   NOT NULL, --'власне ціна, знижка , ...' 
    DATA_ADDITIONAL_CONDITION NUMBER   NOT NULL, --'кількість для ADDITIONAL_CONDITION'
    DATE_CREATE               DATETIME NOT NULL, --'Дата вставки\зміни акції'
    USER_CREATE               INTEGER  NOT NULL  --'Користувач вставки\зміни акції'
);
CREATE INDEX PSD_CODE_WARES ON PROMOTION_SALE_DATA ( CODE_WARES);
CREATE UNIQUE INDEX id_promotion_sale_data ON PROMOTION_SALE_DATA (CODE_PS,NUMBER_GROUP,CODE_WARES);

--'Фільтри для акцій MID i UnicS'
CREATE TABLE PROMOTION_SALE_FILTER (
    CODE_PS           INTEGER  NOT NULL, --'Код акції'
    CODE_FILTER       INTEGER  NOT NULL, --'Код фільтру'
    TYPE_GROUP_FILTER INTEGER  NOT NULL, --'Тип (10- товари 20-Час,30-клієнт,40-Номер чека, 50-склади, 60-Форма оплати, ) ( select t.*, t.rowid from C.DATA_NAME t where t.data_level=53)'
    RULE_GROUP_FILTER INTEGER  NOT NULL, --'Правило групи ( 1- логіче &  , -1 - Заперечення'
    CODE_PROPERTY     INTEGER  NOT NULL, --'Для фільтрів по властивостям код додаткової властивості'
    CODE_CHOICE       INTEGER  NOT NULL, --'Код з CHOICE_DATA'
    DATE_CREATE       DATETIME NOT NULL, --'Дата створення\зміни запису'
    USER_CREATE       INTEGER  NOT NULL  --'користувач який створив\змінив запис'
);
CREATE UNIQUE INDEX id_promotion_sale_filter ON PROMOTION_SALE_FILTER (CODE_FILTER);

--'Подарки по акціям MID i UnicS';
CREATE TABLE PROMOTION_SALE_GIFT (
    CODE_PS       INTEGER  NOT NULL, --'Код акції'
    NUMBER_GROUP  INTEGER  NOT NULL, --'Номер групи'
    CODE_DATA     INTEGER  NOT NULL,
    TYPE_DISCOUNT INTEGER  NOT NULL, --'Тип знижки (1-ціна,2-знижка,3-%знижки, 4- заміна ДК) і тд (select t.*, t.rowid from C.DATA_NAME t where t.data_level=50)';
    DATA          NUMBER   NOT NULL, --'власне ціна, знижка , ...' 
    QUANTITY      NUMBER   NOT NULL, -- К-ть необхідна для подарка
    DATE_CREATE   DATETIME NOT NULL,
    USER_CREATE   INTEGER  NOT NULL
);
CREATE UNIQUE INDEX id_promotion_sale_gift ON PROMOTION_SALE_GIFT (CODE_PS, NUMBER_GROUP, CODE_DATA);




--!!!Region

-- Переклади
CREATE TABLE TRANSLATION (
    NAME        TEXT     NOT NULL,
    LANGUAGE    INTEGER  NOT NULL,
    TRANSLATION TEXT     NOT NULL,
    EX_INFO     TEXT,
    DESCRIPTION TEXT,
    DATE_CREATE DATETIME NOT NULL,
    USER_CREATE INTEGER  NOT NULL
);
CREATE UNIQUE INDEX id_translation ON TRANSLATION (NAME, LANGUAGE);


-- Довідник Одиниць виміру
-- MSSQL dbo.V_UNIT_DIMENSION 
CREATE TABLE UNIT_DIMENSION (
    CODE_UNIT                     INTEGER NOT NULL,
    NAME_UNIT                     TEXT    NOT NULL,
    ABR_UNIT                      TEXT    NOT NULL,
    SIGN_ACTIVITY                 TEXT    NOT NULL,
    SIGN_DIVISIONAL               TEXT    NOT NULL,
    REUSABLE_CONTAINER            TEXT    NOT NULL,
    NUMBER_UNIT                   INTEGER NOT NULL,
    CODE_WARES_REUSABLE_CONTAINER INTEGER,
    DESCRIPTION                   TEXT
);
CREATE UNIQUE INDEX id_UNIT_DIMENSION ON UNIT_DIMENSION ( CODE_UNIT);

-- Користувачі

CREATE TABLE USERS (
    CODE_USER   INTEGER  NOT NULL,
    CODE_UP     INTEGER  NOT NULL,
    TYPE_USER   INTEGER  NOT NULL,
    LOGIN       TEXT     NOT NULL,
    PASSWORD    TEXT     NOT NULL,
    DATE_BEGIN  DATETIME NOT NULL,
    DATE_END    DATETIME NOT NULL,
    DESCRIPTION TEXT,
    DATE_CREATE DATETIME NOT NULL,
    USER_CREATE INTEGER  NOT NULL
);
CREATE UNIQUE INDEX id_users ON USERS (CODE_USER);


CREATE TABLE USERS_ACCESS (
    CODE_USER    INTEGER  NOT NULL,
    LEVEL_ACCESS INTEGER  NOT NULL,
    CODE_ACCESS  INTEGER  NOT NULL,
    TYPE_ACCESS  INTEGER  NOT NULL,
    DATE_CREATE  DATETIME NOT NULL,
    USER_CREATE  INTEGER  NOT NULL
);

CREATE UNIQUE INDEX id_users_access ON USERS_ACCESS (CODE_USER,LEVEL_ACCESS,CODE_ACCESS,TYPE_ACCESS);

CREATE TABLE USERS_LINK (
    CODE_USER_TO   INTEGER  NOT NULL,
    CODE_USER_FROM INTEGER  NOT NULL,
    DATE_CREATE    DATETIME NOT NULL,
    USER_CREATE    INTEGER  NOT NULL
);
CREATE UNIQUE INDEX id_users_link ON USERS_LINK (CODE_USER_TO,CODE_USER_FROM);


---

CREATE TABLE CONVERT_DEALER (
    CODE_WAREHOUSE INTEGER NOT NULL,
    DEALER_FROM    INTEGER NOT NULL,
    DEALER_TO      NUMBER
);

-- Конвертація ДК
CREATE INDEX idx_CONVERT_DEALER ON CONVERT_DEALER (
    CODE_WAREHOUSE,
    DEALER_FROM
);

--Товари
--dbo.v_WARES
CREATE TABLE WARES (
    CODE_WARES          INTEGER  NOT NULL,
    CODE_GROUP          INTEGER  NOT NULL,
    NAME_WARES          TEXT     NOT NULL,
    --TYPE_POSITION_WARES TEXT     NOT NULL,
--    ARTICL              TEXT     NOT NULL,
--    CODE_BRAND          INTEGER  NOT NULL,
--    NAME_WARES_BRAND    TEXT,
--    ARTICL_WARES_BRAND  TEXT,
    CODE_UNIT           INTEGER  NOT NULL,
--    OLD_WARES           TEXT     NOT NULL,
    DESCRIPTION         TEXT,
--    SIGN_1              NUMBER,
--    SIGN_2              NUMBER,
--    SIGN_3              NUMBER,
--    OLD_ARTICL          TEXT,
    VAT                 NUMBER   NOT NULL,
    VAT_OPERATION       NUMBER   NOT NULL,
--    OFF_STOCK_METHOD    TEXT     NOT NULL,
    NAME_WARES_RECEIPT  TEXT,
    --CODE_WARES_RELATIVE INTEGER,
    DATE_INSERT         DATETIME NOT NULL,
    USER_INSERT         INTEGER  NOT NULL,
    --CODE_TRADE_MARK     INTEGER--,
--    KEEPING_TIME        NUMBER
);
CREATE UNIQUE INDEX id_WARES ON WARES ( CODE_WARES);


--Робочі місця
CREATE TABLE WORKPLACE (
    ID_WORKPLACE       INTEGER NOT NULL,
    CODE_WAREHOUSE     INTEGER NOT NULL,
    CODE_CASH_REGISTER INTEGER,
    TRANSFER_MODE      TEXT,
    DESCRIPTION        TEXT
);

CREATE INDEX id_WORKPLACE ON WORKPLACE (ID_WORKPLACE);








