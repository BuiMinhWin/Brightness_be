-- PostgreSQL Database Initialization Script
-- This script runs automatically when the database is first created
-- It also handles cleanup of deprecated schemas

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "vector";

-- Create schemas for all active services
CREATE SCHEMA IF NOT EXISTS authentication;
CREATE SCHEMA IF NOT EXISTS measurement;
CREATE SCHEMA IF NOT EXISTS challenge;
CREATE SCHEMA IF NOT EXISTS social;
CREATE SCHEMA IF NOT EXISTS billing;
CREATE SCHEMA IF NOT EXISTS media;
CREATE SCHEMA IF NOT EXISTS chat;
CREATE SCHEMA IF NOT EXISTS aiocr;
CREATE SCHEMA IF NOT EXISTS personalcoach;

-- Drop deprecated schemas from removed WorkoutAPI and NutritionAPI
-- PersonalCoachAPI now handles all workout and nutrition functionality
DROP SCHEMA IF EXISTS workout CASCADE;
DROP SCHEMA IF EXISTS nutrition CASCADE;

-- Log completion
DO $$ 
BEGIN 
    RAISE NOTICE 'Database initialization completed successfully';
    RAISE NOTICE 'Active schemas: authentication, measurement, challenge, social, billing, media, chat, aiocr, personalcoach';
    RAISE NOTICE 'Removed deprecated schemas: workout, nutrition';
END $$;

