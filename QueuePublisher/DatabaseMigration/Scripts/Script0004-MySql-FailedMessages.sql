﻿----Alerting groups of failed messages
--create table AlertEmails (
--AlertEmailId INT AUTO_INCREMENT PRIMARY KEY,
--Email VARCHAR(255) not null,
--Description VARCHAR(255)
--);

----Failed messages table
--create table FailedMessages (
--FailedMessageId int AUTO_INCREMENT PRIMARY KEY,
--Message varchar(30000) not null,
--CreateTimeStamp DateTime not null,
--LastRetryTimeStamp DateTime not null,
--RetryTicker int not null,
--SecondsDelayInRetry int not null,
--AlertEmailId int not null,
--Foreign Key (AlertEmailId) REFERENCES AlertEmails(AlertEmailId) ON DELETE CASCADE
--) engine=innodb;