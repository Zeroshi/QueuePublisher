﻿--create table EmailContactAlerts (
--EmailContactAlertsId int auto_increment primary key,
--Email varchar(255) not null,
--Description varchar(255)
--);

--create table FailedMessages (
--FailedMessageId int auto_increment primary key,
--SendingApplication varchar(50) not null,
--QueueName varchar(30) not null,
--Message varchar(30000) not null,
--CreatedTimeStamp DateTime not null,
--LastRetryTimeStamp DateTime not null,
--RetryTicker int not null,
--SecondsDelayForRetry int not null,
--AlertEmailId int not null,
--Foreign Key (AlertEmailId) References EmailContactAlerts(EmailContactAlertsId) on delete cascade
--) engine=innodb;

--insert into EmailContactAlerts (Email, Description)
--	values ("email", "description"), ("email2", "description");