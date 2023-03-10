
CREATE SCHEMA `EtherDb` ;
CREATE TABLE Blocks (
  blockID INT(20) NOT NULL AUTO_INCREMENT PRIMARY KEY,
  blockNumber INT(20) NOT NULL,
  hash VARCHAR(66) NOT NULL,
  parentHash VARCHAR(66) NOT NULL,
  miner VARCHAR(42) NOT NULL,
  blockReward DECIMAL(50,0) NOT NULL,
  gasLimit DECIMAL(50,0) NOT NULL,
  gasUsed DECIMAL(50,0) NOT NULL
);

CREATE TABLE Transactions (
  transactionID INT(20) NOT NULL AUTO_INCREMENT PRIMARY KEY,
  blockID INT(20) NOT NULL,
  hash VARCHAR(66) NOT NULL,
  fromAddress VARCHAR(42) NOT NULL,  #change due to keyword
  toAddress VARCHAR(42) NOT NULL,    #change due to keyword
  value DECIMAL(50,0) NOT NULL,
  gas DECIMAL(50,0) NOT NULL,
  gasPrice DECIMAL(50,0) NOT NULL,
  transactionIndex INT(20) NOT NULL,
  FOREIGN KEY (blockID) REFERENCES blocks(blockID)
);
