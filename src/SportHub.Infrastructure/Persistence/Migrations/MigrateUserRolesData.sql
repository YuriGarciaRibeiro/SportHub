-- Migration Script: Update existing UserRole values to new enum structure
-- This script updates User roles from old values to new hierarchical values
--
-- Old Values → New Values:
-- User (0) → Customer (0) - NO CHANGE NEEDED
-- EstablishmentMember (1) → Staff (1) - NO CHANGE NEEDED  
-- Admin (2) → Owner (3) - NEEDS UPDATE
-- SuperAdmin (99) → SuperAdmin (99) - NO CHANGE NEEDED
--
-- Note: This migration assumes the old enum had:
-- - User = 0
-- - EstablishmentMember = 1
-- - Admin = 2
-- - SuperAdmin = 99
--
-- The new enum has:
-- - Customer = 0
-- - Staff = 1
-- - Manager = 2
-- - Owner = 3
-- - SuperAdmin = 99

-- Execute this script AFTER applying the EF Core migration

DO $$
DECLARE
    schema_name TEXT;
    updated_count INTEGER;
BEGIN
    RAISE NOTICE 'Starting UserRole data migration...';
    
    -- Iterate through all tenant schemas (excluding system schemas)
    FOR schema_name IN 
        SELECT nspname 
        FROM pg_namespace 
        WHERE nspname NOT IN ('pg_catalog', 'information_schema', 'public')
        AND nspname NOT LIKE 'pg_%'
        AND nspname NOT LIKE 'pg_toast%'
    LOOP
        RAISE NOTICE 'Processing schema: %', schema_name;
        
        -- Update Admin (2) → Owner (3)
        EXECUTE format('
            UPDATE %I."Users" 
            SET "Role" = 3 
            WHERE "Role" = 2 
            AND "IsDeleted" = false
        ', schema_name);
        
        GET DIAGNOSTICS updated_count = ROW_COUNT;
        
        IF updated_count > 0 THEN
            RAISE NOTICE 'Updated % users from Admin to Owner in schema %', updated_count, schema_name;
        END IF;
        
        -- Verify no unexpected role values exist
        EXECUTE format('
            SELECT COUNT(*) 
            FROM %I."Users" 
            WHERE "Role" NOT IN (0, 1, 2, 3, 99)
            AND "IsDeleted" = false
        ', schema_name) INTO updated_count;
        
        IF updated_count > 0 THEN
            RAISE WARNING 'Found % users with unexpected role values in schema %', updated_count, schema_name;
        END IF;
    END LOOP;
    
    RAISE NOTICE 'UserRole data migration completed successfully!';
END $$;

-- Verification query (run after migration)
-- SELECT nspname as schema, "Role", COUNT(*) as total
-- FROM pg_namespace 
-- CROSS JOIN LATERAL (
--     SELECT "Role" FROM <schema>."Users" WHERE "IsDeleted" = false
-- ) u
-- WHERE nspname NOT IN ('pg_catalog', 'information_schema', 'public')
-- AND nspname NOT LIKE 'pg_%'
-- GROUP BY nspname, "Role"
-- ORDER BY nspname, "Role";
