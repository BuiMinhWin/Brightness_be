-- Check embedding generation progress
SELECT 
    COUNT(*) FILTER (WHERE "EmbeddingVector" IS NOT NULL) as with_embeddings,
    COUNT(*) FILTER (WHERE "EmbeddingVector" IS NULL) as without_embeddings,
    COUNT(*) as total,
    ROUND(100.0 * COUNT(*) FILTER (WHERE "EmbeddingVector" IS NOT NULL) / COUNT(*), 2) as progress_percent
FROM exercise_library;
