# Personal Fitness Coach - Database Schema Design

## 1. User Profile & Health Data

### `user_profiles`
Stores comprehensive user information for personalized coaching

```sql
- id (uuid, PK)
- user_id (uuid, FK to users)
- full_name (varchar)
- date_of_birth (date)
- gender (varchar)
- height_cm (decimal)
- current_weight_kg (decimal)
- target_weight_kg (decimal)
- fitness_level (enum: beginner, intermediate, advanced)
- fitness_goal (enum: lose_weight, gain_muscle, maintain, improve_endurance)
- created_at (timestamp)
- updated_at (timestamp)
```

### `user_health_metrics`
Stores InBody and other health measurements

```sql
- id (uuid, PK)
- user_id (uuid, FK)
- measurement_date (timestamp)
- weight_kg (decimal)
- muscle_mass_kg (decimal)
- body_fat_percentage (decimal)
- body_fat_kg (decimal)
- bmr_kcal (int)
- water_percentage (decimal)
- bone_mass_kg (decimal)
- visceral_fat_level (int)
- metabolic_age (int)
- source (varchar) - inbody, manual, smart_scale
- raw_data_json (jsonb) - full InBody report
- created_at (timestamp)
```

### `user_preferences`
User workout and nutrition preferences

```sql
- id (uuid, PK)
- user_id (uuid, FK)
- available_days (jsonb) - [monday, tuesday, ...]
- preferred_workout_time (varchar) - morning, afternoon, evening
- workout_duration_minutes (int)
- equipment_available (jsonb) - [dumbbells, barbell, machine, ...]
- gym_access (boolean)
- home_workout_only (boolean)
- dietary_restrictions (jsonb) - allergies, vegetarian, etc
- disliked_exercises (jsonb)
- favorite_exercises (jsonb)
- created_at (timestamp)
- updated_at (timestamp)
```

## 2. AI-Generated Schedules

### `workout_schedules`
Master schedule table

```sql
- id (uuid, PK)
- user_id (uuid, FK)
- schedule_name (varchar)
- start_date (date)
- end_date (date)
- duration_days (int) - 7 or 14
- goal (varchar)
- difficulty_level (varchar)
- status (enum: active, completed, paused, cancelled)
- ai_generation_prompt (text)
- created_by_ai (boolean)
- created_at (timestamp)
- updated_at (timestamp)
```

### `workout_schedule_days`
Individual days in the schedule

```sql
- id (uuid, PK)
- schedule_id (uuid, FK)
- day_number (int) - 1-14
- date (date)
- day_type (enum: workout, rest, active_recovery)
- focus_area (varchar) - upper_body, lower_body, cardio, full_body
- completed (boolean)
- completion_date (timestamp)
- user_notes (text)
- ai_modified (boolean) - track if AI modified this day
- modification_count (int)
- created_at (timestamp)
- updated_at (timestamp)
```

### `workout_schedule_exercises`
Exercises for each day

```sql
- id (uuid, PK)
- schedule_day_id (uuid, FK)
- exercise_name (varchar)
- exercise_category (varchar) - strength, cardio, flexibility
- target_muscles (jsonb)
- sets (int)
- reps (varchar) - can be range like "8-12"
- duration_seconds (int) - for time-based exercises
- rest_seconds (int)
- weight_kg (decimal) - suggested weight
- order_index (int)
- notes (text)
- video_url (varchar)
- completed (boolean)
- actual_sets (int)
- actual_reps (int)
- actual_weight_kg (decimal)
- created_at (timestamp)
- updated_at (timestamp)
```

## 3. Knowledge Base for RAG

### `exercise_library`
Exercise knowledge base for RAG

```sql
- id (uuid, PK)
- exercise_name (varchar)
- exercise_name_vi (varchar)
- category (varchar)
- difficulty_level (varchar)
- equipment_required (jsonb)
- target_muscles (jsonb)
- secondary_muscles (jsonb)
- description (text)
- description_vi (text)
- instructions (jsonb) - step by step
- benefits (jsonb)
- common_mistakes (jsonb)
- variations (jsonb)
- video_url (varchar)
- image_url (varchar)
- calories_per_minute (decimal)
- embedding_vector (vector) - for semantic search
- created_at (timestamp)
```

### `health_knowledge_base`
Medical and nutrition knowledge for RAG

```sql
- id (uuid, PK)
- category (enum: nutrition, medical, fitness, recovery)
- topic (varchar)
- content (text)
- content_vi (text)
- source (varchar)
- reliability_score (decimal)
- tags (jsonb)
- embedding_vector (vector)
- created_at (timestamp)
```

## 4. AI Conversation & Context

### `ai_conversations`
Chat history for context-aware responses

```sql
- id (uuid, PK)
- user_id (uuid, FK)
- session_id (uuid)
- message_type (enum: user, assistant, system)
- message_content (text)
- context_used (jsonb) - what data was used for this response
- rag_results (jsonb) - retrieved knowledge base items
- created_at (timestamp)
```

### `user_context_snapshots`
Periodic snapshots for RAG context

```sql
- id (uuid, PK)
- user_id (uuid, FK)
- snapshot_date (timestamp)
- health_summary (text)
- fitness_summary (text)
- goals_summary (text)
- progress_summary (text)
- embedding_vector (vector)
- created_at (timestamp)
```

## 5. Progress Tracking

### `workout_logs`
Track actual workout completion

```sql
- id (uuid, PK)
- user_id (uuid, FK)
- schedule_day_id (uuid, FK)
- workout_date (date)
- completion_rate (decimal) - 0.0 to 1.0
- user_rating (int) - 1-5
- difficulty_rating (int) - 1-5
- notes (text)
- duration_minutes (int)
- calories_burned (int)
- created_at (timestamp)
```

### `user_achievements`
Track milestones and achievements

```sql
- id (uuid, PK)
- user_id (uuid, FK)
- achievement_type (varchar)
- achievement_name (varchar)
- achievement_date (timestamp)
- description (text)
- created_at (timestamp)
```

## 6. Calendar Integration

### `calendar_sync`
Google Calendar synchronization

```sql
- id (uuid, PK)
- user_id (uuid, FK)
- google_calendar_id (varchar)
- access_token (text) - encrypted
- refresh_token (text) - encrypted
- token_expiry (timestamp)
- sync_enabled (boolean)
- last_sync (timestamp)
- created_at (timestamp)
```

### `calendar_events`
Mapped workout events to Google Calendar

```sql
- id (uuid, PK)
- user_id (uuid, FK)
- schedule_day_id (uuid, FK)
- google_event_id (varchar)
- event_title (varchar)
- event_description (text)
- start_time (timestamp)
- end_time (timestamp)
- synced (boolean)
- created_at (timestamp)
- updated_at (timestamp)
```

## 7. Nutrition Planning (FR-008)

### `nutrition_plans`
Nutrition plans linked to workout schedules

```sql
- id (uuid, PK)
- user_id (uuid, FK)
- schedule_id (uuid, FK)
- plan_name (varchar)
- daily_calorie_target (int)
- protein_g (decimal)
- carbs_g (decimal)
- fat_g (decimal)
- meal_count (int)
- dietary_restrictions (jsonb)
- created_at (timestamp)
```

### `nutrition_plan_days`
Daily nutrition breakdown

```sql
- id (uuid, PK)
- nutrition_plan_id (uuid, FK)
- day_number (int)
- date (date)
- calorie_target (int)
- macro_breakdown (jsonb)
- meal_suggestions (jsonb)
- completed (boolean)
- created_at (timestamp)
```

## 8. Smart Adjustments (FR-009)

### `schedule_adjustments`
Track AI-driven schedule modifications

```sql
- id (uuid, PK)
- user_id (uuid, FK)
- schedule_id (uuid, FK)
- adjustment_date (timestamp)
- reason (varchar) - completion_rate, user_feedback, performance
- old_difficulty (varchar)
- new_difficulty (varchar)
- changes_made (jsonb)
- ai_reasoning (text)
- created_at (timestamp)
```

### `user_feedback`
User ratings and feedback for adjustments

```sql
- id (uuid, PK)
- user_id (uuid, FK)
- schedule_day_id (uuid, FK)
- rating (int) - 1-5
- difficulty_rating (int) - 1-5 (too easy to too hard)
- feedback_text (text)
- created_at (timestamp)
```

## Key Features:

1. **User Profile**: Complete health and fitness data
2. **Health Metrics**: InBody and measurement tracking
3. **AI Schedules**: Generated workout plans
4. **RAG Knowledge Base**: Exercise library with vector search
5. **Context Tracking**: Conversation history for personalization
6. **Calendar Integration**: Google Calendar sync
7. **Progress Tracking**: Completion rates and achievements
8. **Smart Adjustments**: AI-driven difficulty modifications
9. **Nutrition Planning**: Integrated meal planning
10. **Flexible Editing**: Modify specific days without full regeneration
