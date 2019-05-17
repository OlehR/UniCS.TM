-- Create table
create table UCS.DATA
(
  id_data             INTEGER not null,
  code_up             INTEGER not null,
  name_data           VARCHAR2(40),
  --type_route          INTEGER not null,
  type_create_package INTEGER not null, --
  --type_load_package   INTEGER not null,
  description         VARCHAR2(4000),
  --type_keep           INTEGER,
  --type_load_data      INTEGER,
  id_1                VARCHAR2(32),
  id_2                VARCHAR2(32),
  id_3                VARCHAR2(32),
  filter_field_1      VARCHAR2(32),
  filter_field_2      VARCHAR2(32),
  filter_field_3      VARCHAR2(32),
  convert_data        VARCHAR2(4000),
  main_table          VARCHAR2(40),
  id_1_main           VARCHAR2(32),
  id_2_main           VARCHAR2(32),
  id_3_main           VARCHAR2(32),
  object_id           NUMBER,
  addition_n1         NUMBER,
  addition_n2         NUMBER,
  addition_n3         NUMBER,
  addition_n4         NUMBER,
  addition_n5         NUMBER,
  addition_с1         VARCHAR2(4000),
  addition_с2         VARCHAR2(4000),
  addition_с3         VARCHAR2(4000),
  addition_d1         DATE,
  addition_d2         DATE,
  addition_d3         DATE,
  add_index           VARCHAR2(4000),
  code_file           INTEGER default 0 not null,
  type_node_source    INTEGER default 0 not null,
  type_node_filter    INTEGER default 0 not null
)
tablespace C
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 80K
    next 1M
    minextents 1
    maxextents unlimited
  );
-- Add comments to the table 
comment on table UCS.DATA
  is 'Інформація про дані для обміну.';
-- Add comments to the columns 
comment on column UCS.DATA.id_data
  is 'ID';
comment on column UCS.DATA.code_up
  is 'ID головної таблички аналогічно MAIN_TABLE';
comment on column UCS.DATA.name_data
  is 'Повна назва таблички (SPR.WARES)';
comment on column UCS.DATA.type_route
  is ' (select t.*, t.rowid from UCS.LIST_ATTRIBUTE t where t.data_level=3)';
comment on column UCS.DATA.description
  is 'Опис даних';
comment on column UCS.DATA.type_keep
  is '(-2 - видаляти до останнього акумулятивного,-1 - не видаляти.  ,0-Видаляти після доставки. ,>0 днів';
comment on column UCS.DATA.type_load_data
  is '1-Зовнішня табличка(аудит ), 2 - поле в табличці (Тип Date)';
comment on column UCS.DATA.id_1
  is 'Назва ключа 1 (Code_invoice)';
comment on column UCS.DATA.id_2
  is 'Назва ключа 2 (Code_wares)';
comment on column UCS.DATA.id_3
  is 'Назва ключа 3';
comment on column UCS.DATA.convert_data
  is 'Якщо пусто то всі поля.';
comment on column UCS.DATA.main_table
  is 'Якщо пусто  то вона є головна.';
comment on column UCS.DATA.id_1_main
  is 'Назва ключа 1 звязку з головною (наприклад(code_invoice MZ.wares_invoice))';
comment on column UCS.DATA.id_2_main
  is 'Назва ключа 2 звязку з головною';
comment on column UCS.DATA.id_3_main
  is 'Назва ключа 3 звязку з головною';
comment on column UCS.DATA.object_id
  is 'OBJECT_ID бази даних';
comment on column UCS.DATA.addition_с1
  is 'Додаткові індекси для SQLite';
comment on column UCS.DATA.code_file
  is ' № по порядку згенерованого файла (ucs.proc.getCodeFile()) ';
comment on column UCS.DATA.type_node_source
  is 'Ноди на яких генеруються пакети. (select t.*, t.rowid from UCS.LIST_ATTRIBUTE t where t.data_level=1)';
comment on column UCS.DATA.type_node_filter
  is 'Тип фільтруючої ноди (наприклад Склад, для ДК )  (select t.*, t.rowid from UCS.LIST_ATTRIBUTE t where t.data_level=1)';
-- Create/Recreate primary, unique and foreign key constraints 
alter table UCS.DATA
  add constraint ID_DATA primary key (ID_DATA)
  using index 
  tablespace C_I
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 80K
    next 80K
    minextents 1
    maxextents unlimited
  );

  
  
  -- Create table
create table UCS.ERROR
(
  id_node         INTEGER not null,
  id_package_node INTEGER,
  id_package      INTEGER,
  id_file         VARCHAR2(30),
  row_num         INTEGER,
  type_error      INTEGER not null,
  error_message   VARCHAR2(4000),
  id_route        INTEGER not null,
  description     VARCHAR2(250)
)
tablespace C
  pctfree 10
  initrans 1
  maxtrans 255;

  
  -- Create table
create table UCS.DATA_FILE
(
  id_file    VARCHAR2(30) not null,
  id_data    INTEGER not null,
  id_node    INTEGER not null,
  type_group INTEGER default 0 not null,
  code_group INTEGER default 0 not null,
  sort       INTEGER default 0
)
tablespace C
  pctfree 10
  initrans 1
  maxtrans 255;
-- Add comments to the table 
comment on table UCS.DATA_FILE
  is 'Зберігає зформовані файли';
-- Add comments to the columns 
comment on column UCS.DATA_FILE.id_file
  is 'Назва файла';
comment on column UCS.DATA_FILE.id_data
  is 'Id - даних';
comment on column UCS.DATA_FILE.id_node
  is 'Нода джерело';
comment on column UCS.DATA_FILE.type_group
  is 'Тип розбиття ( ID_DATA довідника)';
comment on column UCS.DATA_FILE.code_group
  is 'Код';
comment on column UCS.DATA_FILE.sort
  is 'Номер файла по порядку.';
-- Create/Recreate primary, unique and foreign key constraints 
alter table UCS.DATA_FILE
  add constraint ID_DATA_FILE primary key (ID_FILE)
  using index 
  tablespace C_I
  pctfree 10
  initrans 2
  maxtrans 255;
alter table UCS.DATA_FILE
  add constraint DATA_FILE_N foreign key (ID_NODE)
  references UCS.NODE (ID_NODE);

  
  
  
  

-- Create table
create table UCS.NODE
(
  id_node        INTEGER not null,
  name_node      VARCHAR2(250) not null,
  id_parent_node INTEGER not null,
  type_node      INTEGER not null,
  state_node     INTEGER not null,
  ip_node        VARCHAR2(15),
  description    VARCHAR2(250)
)
tablespace C
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 80K
    next 1M
    minextents 1
    maxextents unlimited
  );
-- Add comments to the table 
comment on table UCS.NODE
  is 'Список вузлів';
-- Add comments to the columns 
comment on column UCS.NODE.id_node
  is 'ID вузла';
comment on column UCS.NODE.name_node
  is 'Назва ноди';
comment on column UCS.NODE.id_parent_node
  is 'Батьківська нода (???)';
comment on column UCS.NODE.type_node
  is 'select t.*, t.rowid from UCS.LIST_ATTRIBUTE t where t.data_level=1';
comment on column UCS.NODE.state_node
  is 'select t.*, t.rowid from UCS.LIST_ATTRIBUTE t where t.data_level=2';
comment on column UCS.NODE.description
  is 'Опис вузла';
-- Create/Recreate primary, unique and foreign key constraints 
alter table UCS.NODE
  add constraint ID_N primary key (ID_NODE)
  using index 
  tablespace C_I
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 80K
    next 1M
    minextents 1
    maxextents unlimited
  );
alter table UCS.NODE
  add constraint ID_PARENT_N foreign key (ID_PARENT_NODE)
  references UCS.NODE (ID_NODE);
  
  
  -- Create table
create table UCS.PACKAGE
(
  id_node      INTEGER not null,
  id_session   INTEGER not null,
  id_package   INTEGER not null,
  code_package VARCHAR2(30) not null,
  code_file    VARCHAR2(50) not null
)
tablespace C
  pctfree 10
  initrans 1
  maxtrans 255;
-- Add comments to the columns 
comment on column UCS.PACKAGE.id_node
  is 'Нода, яка згенерувала файл.';
comment on column UCS.PACKAGE.id_session
  is 'Код сесії створення обєднує всі пакети створені за один раз.';
comment on column UCS.PACKAGE.id_package
  is 'ID пакета сіквенс???';
comment on column UCS.PACKAGE.code_package
  is 'PK_000001_00000011_3_2_002317
000001 - id_node
00000011 - № по порядку (необхідно сіквенс??)
3- тип ноди отримувача (select t.*, t.rowid from UCS.LIST_ATTRIBUTE t where t.data_level=1)
2- тип фільтруючої ноди. (Наприклад price_dealer фільтрується по складу) Якщо фільтрація непотрібна - 0, інакше код обєкта фільтрації (наприклад код складу) UCS.DATA.TYPE_NODE_FILTER.
002317 - код об’єкта фільтрації.(Якщо непотрібно -000000)';
comment on column UCS.PACKAGE.code_file
  is 'формат виду wares_000001_00000011.csv';
-- Create/Recreate primary, unique and foreign key constraints 
alter table UCS.PACKAGE
  add constraint ID_PACKAGE_IN_ROUTE primary key (CODE_FILE, ID_NODE, ID_PACKAGE, CODE_PACKAGE)
  using index 
  tablespace C_I
  pctfree 10
  initrans 2
  maxtrans 255;
alter table UCS.PACKAGE
  add constraint PACKAGE_IN_ROUTE_F foreign key (CODE_FILE)
  references UCS.DATA_FILE (ID_FILE);
alter table UCS.PACKAGE
  add constraint PACKAGE_IN_ROUTE_N foreign key (ID_NODE)
  references UCS.NODE (ID_NODE);

  
  
  -- Create table
create table UCS.ROUTE
(
  id_route    INTEGER not null,
  state_route INTEGER not null,
  name_route  VARCHAR2(250),
  type_route  INTEGER not null,
  description VARCHAR2(250)
)
tablespace C
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 80K
    next 1M
    minextents 1
    maxextents unlimited
  );
-- Add comments to the table 
comment on table UCS.ROUTE
  is 'Маршрути';
-- Add comments to the columns 
comment on column UCS.ROUTE.id_route
  is 'ID Шляху';
comment on column UCS.ROUTE.state_route
  is 'select t.*, t.rowid from UCS.LIST_ATTRIBUTE t where t.data_level=4';
comment on column UCS.ROUTE.name_route
  is 'Назва шляху.';
comment on column UCS.ROUTE.type_route
  is ' select t.*, t.rowid from UCS.LIST_ATTRIBUTE t where t.data_level=3';
comment on column UCS.ROUTE.description
  is 'Опис.';
-- Create/Recreate primary, unique and foreign key constraints 
alter table UCS.ROUTE
  add constraint ID_ROUTE primary key (ID_ROUTE)
  using index 
  tablespace C_I
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );


  -- Create table
create table UCS.NODE_IN_ROUTE
(
  id_route       INTEGER not null,
  id_node        INTEGER not null,
  id_parent_node INTEGER not null
)
tablespace C
  pctfree 10
  initrans 1
  maxtrans 255;
-- Create/Recreate primary, unique and foreign key constraints 
alter table UCS.NODE_IN_ROUTE
  add constraint ID_NODE_IN_ROUTE primary key (ID_ROUTE, ID_NODE)
  using index 
  tablespace C_I
  pctfree 10
  initrans 2
  maxtrans 255;
alter table UCS.NODE_IN_ROUTE
  add constraint NODE_IN_ROUTE_N foreign key (ID_NODE)
  references UCS.NODE (ID_NODE);
alter table UCS.NODE_IN_ROUTE
  add constraint NODE_IN_ROUTE_P_N foreign key (ID_PARENT_NODE)
  references UCS.NODE (ID_NODE);


  
-- Create/Recreate primary, unique and foreign key constraints 
alter table UCS.DATA_HISTORY
  add constraint ID_DATA_HISTORY primary key (DATE_CHANGE, ID_DATA, ID_1, ID_2, ID_3, ID_NODE)
  using index 
  tablespace C_I
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 80K
    next 1M
    minextents 1
    maxextents unlimited
  );
alter table UCS.DATA_HISTORY
  add constraint DATA_HISTORY_N foreign key (ID_NODE)
  references UCS.NODE (ID_NODE);
