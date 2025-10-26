-- Insert data for subscription_plans table
-- Các gói subscriptions với nhiều mức giá và thời hạn khác nhau

-- Gói miễn phí (Free plan)
INSERT INTO subscription_plans (id, name, description, price, currency, duration_days, is_active, created_at, updated_at)
VALUES 
('11111111-1111-1111-1111-111111111111', 'Free Plan', 'Gói miễn phí với các tính năng cơ bản', 0, 'VND', 30, true, NOW(), NULL);

-- Gói hàng tháng (Monthly plans)
INSERT INTO subscription_plans (id, name, description, price, currency, duration_days, is_active, created_at, updated_at)
VALUES 
('22222222-2222-2222-2222-222222222222', 'Basic Monthly', 'Gói cơ bản với đầy đủ tính năng, thanh toán hàng tháng', 79000, 'VND', 30, true, NOW(), NULL),
('33333333-3333-3333-3333-333333333333', 'Premium Monthly', 'Gói cao cấp với tất cả tính năng và hỗ trợ ưu tiên, thanh toán hàng tháng', 149000, 'VND', 30, true, NOW(), NULL);

-- Gói hàng năm (Yearly plans) - giảm giá so với thanh toán hàng tháng
INSERT INTO subscription_plans (id, name, description, price, currency, duration_days, is_active, created_at, updated_at)
VALUES 
('44444444-4444-4444-4444-444444444444', 'Basic Yearly', 'Gói cơ bản với đầy đủ tính năng, thanh toán hàng năm (tiết kiệm 20%)', 759000, 'VND', 365, true, NOW(), NULL),
('55555555-5555-5555-5555-555555555555', 'Premium Yearly', 'Gói cao cấp với tất cả tính năng và hỗ trợ ưu tiên, thanh toán hàng năm (tiết kiệm 20%)', 1429000, 'VND', 365, true, NOW(), NULL);

-- Gói đặc biệt (Special offers)
INSERT INTO subscription_plans (id, name, description, price, currency, duration_days, is_active, created_at, updated_at)
VALUES 
('66666666-6666-6666-6666-666666666666', 'Student Plan', 'Gói dành cho học sinh, sinh viên với giá ưu đãi', 49000, 'VND', 30, true, NOW(), NULL),
('77777777-7777-7777-7777-777777777777', 'Family Plan', 'Gói dành cho gia đình, hỗ trợ đến 5 tài khoản', 299000, 'VND', 30, true, NOW(), NULL);

-- Gói khuyến mãi (có thể được kích hoạt trong các chiến dịch marketing)
INSERT INTO subscription_plans (id, name, description, price, currency, duration_days, is_active, created_at, updated_at)
VALUES 
('88888888-8888-8888-8888-888888888888', 'Special Promotion', 'Gói khuyến mãi đặc biệt - 3 tháng sử dụng Premium', 299000, 'VND', 90, false, NOW(), NULL),
('99999999-9999-9999-9999-999999999999', 'Summer Deal', 'Ưu đãi mùa hè - 6 tháng sử dụng Premium', 499000, 'VND', 180, false, NOW(), NULL);