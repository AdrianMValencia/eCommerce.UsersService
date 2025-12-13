CREATE DATABASE eCommerceUsersServiceDb;

CREATE TABLE public."Users" (
	"UserID" uuid NOT NULL,
	"FirstName" text NOT NULL,
	"LastName" text NOT NULL,
	"Email" text NOT NULL,
	"Password" text NOT NULL,
	CONSTRAINT "PK_Users" PRIMARY KEY ("UserID")
);