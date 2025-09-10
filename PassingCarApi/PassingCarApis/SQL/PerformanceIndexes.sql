-- Performance Optimization Indexes for PassingCar Database
-- These indexes will significantly improve login and data loading performance

-- Index for User login queries (Email and PhoneNumber lookups)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_User_Email_HashedPassword')
CREATE NONCLUSTERED INDEX IX_User_Email_HashedPassword 
ON [User] (Email, HashedPassword)
INCLUDE (Id, Name, Photo, Uber, Customer, JuridicPerson, GoogleId, FacebookId, AppleId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_User_PhoneNumber_HashedPassword')
CREATE NONCLUSTERED INDEX IX_User_PhoneNumber_HashedPassword 
ON [User] (PhoneNumber, HashedPassword)
INCLUDE (Id, Name, Photo, Uber, Customer, JuridicPerson, GoogleId, FacebookId, AppleId);

-- Index for external login providers
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_User_GoogleId')
CREATE NONCLUSTERED INDEX IX_User_GoogleId 
ON [User] (GoogleId)
INCLUDE (Id, Email, Name, Photo, Uber, Customer, JuridicPerson);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_User_FacebookId')
CREATE NONCLUSTERED INDEX IX_User_FacebookId 
ON [User] (FacebookId)
INCLUDE (Id, Email, Name, Photo, Uber, Customer, JuridicPerson);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_User_AppleId')
CREATE NONCLUSTERED INDEX IX_User_AppleId 
ON [User] (AppleId)
INCLUDE (Id, Email, Name, Photo, Uber, Customer, JuridicPerson);

-- Index for Review queries (user rating calculations)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Review_ReviewedUserId_Profile')
CREATE NONCLUSTERED INDEX IX_Review_ReviewedUserId_Profile 
ON [Review] (ReviewedUserId, ReviewedUserProfile)
INCLUDE (Rating);

-- Index for Notification queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Notification_UserId_Profile')
CREATE NONCLUSTERED INDEX IX_Notification_UserId_Profile 
ON [Notification] (UserId, UserProfile);

-- Index for ValidationCode queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ValidationCode_PhoneNumber_CreatedAt')
CREATE NONCLUSTERED INDEX IX_ValidationCode_PhoneNumber_CreatedAt 
ON [ValidationCode] (PhoneNumber, CreatedAt DESC);

-- Index for Ads queries (user's own ads)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Ads_UserId_UserProfile')
CREATE NONCLUSTERED INDEX IX_Ads_UserId_UserProfile 
ON [Ads] (UserId, UserProfile)
INCLUDE (Id, Title, [From], [To], Price, State, CreatedAt, ModifiedAt);

-- Index for Ads state and date queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Ads_State_ModifiedAt')
CREATE NONCLUSTERED INDEX IX_Ads_State_ModifiedAt 
ON [Ads] (State, ModifiedAt)
INCLUDE (Id, Photo1, Photo2, Photo3, Photo4);

-- Index for Ads date-based cleanup queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Ads_CreatedAt')
CREATE NONCLUSTERED INDEX IX_Ads_CreatedAt 
ON [Ads] (CreatedAt)
INCLUDE (Id);

-- Index for Chat queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Chat_AdId')
CREATE NONCLUSTERED INDEX IX_Chat_AdId 
ON [Chat] (AdId);

-- Index for Offer queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Offer_AdId')
CREATE NONCLUSTERED INDEX IX_Offer_AdId 
ON [Offer] (AdId)
INCLUDE (Id);

-- Index for Shipping queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Shipping_OfferId')
CREATE NONCLUSTERED INDEX IX_Shipping_OfferId 
ON [Shipping] (OfferId);

-- Index for Payment queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Payment_OfferId')
CREATE NONCLUSTERED INDEX IX_Payment_OfferId 
ON [Payment] (OfferId);

-- Index for FavoriteAds queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FavoriteAds_AdId')
CREATE NONCLUSTERED INDEX IX_FavoriteAds_AdId 
ON [FavoriteAds] (AdId);

-- Index for ChatMessage queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ChatMessage_ChatId')
CREATE NONCLUSTERED INDEX IX_ChatMessage_ChatId 
ON [ChatMessage] (ChatId);

PRINT 'Performance indexes created successfully!';
