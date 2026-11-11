-- ==================================================================
-- Script  : database/schema.sql
-- Fecha   : 2026-11-11
-- Autor   : gobernalc
-- Resumen : Esquema único e idempotente de la base de datos TaskPage.
--           Debe poder ejecutarse N veces sin efectos colaterales y
--           funcionar en cualquier instancia de SQL Server.
-- ==================================================================
-- ------------------------------------------------------------------
-- Sección 1: creación de la base de datos.
-- ------------------------------------------------------------------
IF DB_ID(N'TaskPageDb') IS NULL
    BEGIN
        CREATE DATABASE TaskPageDb;
    END


GO
USE TaskPageDb;