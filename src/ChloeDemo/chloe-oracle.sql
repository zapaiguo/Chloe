
-- ----------------------------
-- Table structure for CITY
-- ----------------------------
DROP TABLE "SYSTEM"."CITY";
CREATE TABLE "SYSTEM"."CITY" (
"ID" NUMBER NOT NULL ,
"NAME" NVARCHAR2(255) NULL ,
"PROVINCEID" NUMBER NULL 
)
LOGGING
NOCOMPRESS
NOCACHE

;

-- ----------------------------
-- Indexes structure for table CITY
-- ----------------------------

-- ----------------------------
-- Checks structure for table CITY
-- ----------------------------
ALTER TABLE "SYSTEM"."CITY" ADD CHECK ("ID" IS NOT NULL);

-- ----------------------------
-- Primary Key structure for table CITY
-- ----------------------------
ALTER TABLE "SYSTEM"."CITY" ADD PRIMARY KEY ("ID");



-- ----------------------------
-- Table structure for USERS
-- ----------------------------
DROP TABLE "SYSTEM"."USERS";
CREATE TABLE "SYSTEM"."USERS" (
"ID" NUMBER NOT NULL ,
"NAME" NVARCHAR2(255) NULL ,
"AGE" NUMBER NULL ,
"CITYID" NUMBER NULL ,
"OPTIME" DATE NULL ,
"GENDER" NUMBER NULL 
)
LOGGING
NOCOMPRESS
NOCACHE

;


ALTER TABLE "SYSTEM"."USERS" ADD CHECK ("ID" IS NOT NULL);


ALTER TABLE "SYSTEM"."USERS" ADD PRIMARY KEY ("ID");

 
-- ----------------------------
-- Table structure for PROVINCE
-- ----------------------------
DROP TABLE "SYSTEM"."PROVINCE";
CREATE TABLE "SYSTEM"."PROVINCE" (
"ID" NUMBER NOT NULL ,
"NAME" NVARCHAR2(255) NULL 
)
LOGGING
NOCOMPRESS
NOCACHE

ALTER TABLE "SYSTEM"."PROVINCE" ADD CHECK ("ID" IS NOT NULL);

ALTER TABLE "SYSTEM"."PROVINCE" ADD PRIMARY KEY ("ID");


CREATE OR REPLACE 
function MyFunction(id INTEGER)
 return VARCHAR
as
begin
   return 'id: ' || "TO_CHAR"(id);
end;
