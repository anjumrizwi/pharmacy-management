# VB6 to C# Migration Notes

## What was migrated

A replacement C# WinForms application was created in this folder:
- PharmacyManagement.CSharp

This C# app keeps the original functional structure:
- Login screen (legacy credentials supported)
- Main module menu
- CRUD screens for all major tables in the original project

## Module mapping from VB6

- Form1.frm (loginn) -> Forms/LoginForm.cs
- MDIForm1.frm -> Forms/MainForm.cs
- Form2.frm (billl) -> Forms/CrudForm.cs with table bill
- Form3.frm (customerr) -> Forms/CrudForm.cs with table cust
- Form4.frm (doctorr) -> Forms/CrudForm.cs with table doc
- Form5.frm (supplierr) -> Forms/CrudForm.cs with table supplier
- Form6.frm (orderr) -> Forms/CrudForm.cs with table order1
- Form7.frm (stockk) -> Forms/CrudForm.cs with table stock
- Form8.frm (sinvoicee) -> Forms/CrudForm.cs with table sinvoice
- medd.frm (medd) -> Forms/CrudForm.cs with table med
- Form10.frm (about) -> About dialog in Forms/MainForm.cs

## Database

The C# app uses Microsoft Access via OleDb.

Connection source priority:
1. Environment variable PHARMACY_DB_PATH (full path to pharm.mdb)
2. pharm.mdb in app output folder

Example environment variable (PowerShell):
$env:PHARMACY_DB_PATH = "D:\path\to\pharm.mdb"

## Run

From this folder run:
- dotnet build
- dotnet run

## Important differences vs VB6

- SQL writes are parameterized to reduce SQL injection and quoting issues.
- One reusable CRUD form is used for all data modules instead of separate duplicated form code.
- VB6 DataReport/DataEnvironment designer artifacts are not migrated as-is.
  For reporting in C#, add RDLC, FastReport, or another reporting library as next step.

## Default login (same as VB6)

- Username: admin
- Email: mail@admin
- Password: #access
- Legacy quick-fill: type a in the helper box
