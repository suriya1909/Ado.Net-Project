Create database Project_DB
go
use Project_DB
go
CREATE TABLE Positions
(
    PositionId INT          IDENTITY (1, 1) NOT NULL,
    Position   VARCHAR (50) NULL,
    PRIMARY KEY CLUSTERED (PositionId ASC)
);
go
INSERT INTO Positions VALUES
('Manger'),
('Sr. Executive'),
('Executive')
go
CREATE TABLE Employee 
(
    EmpId INT IDENTITY (1, 1) NOT NULL,
    EmpCod NVARCHAR (30) NULL,
    EmpName NVARCHAR (50) NULL,
    PositionId INT NULL,
    DOB DATETIME NULL,
    Gender VARCHAR (20)  NULL,
    Status VARCHAR (15)  NULL,
    imagePath  VARCHAR (MAX) NULL,
    PRIMARY KEY CLUSTERED (EmpId ASC),
    FOREIGN KEY (PositionId) REFERENCES Positions(PositionId)
);

go
CREATE TABLE Company
(
    EmpCmpId   INT           IDENTITY (1, 1) NOT NULL,
    EmpCmpName NVARCHAR (50) NULL,
    PositionId INT           NULL,
    EmpId      INT           NULL,
    YearOfExp  INT           NULL,
    PRIMARY KEY CLUSTERED (EmpCmpId ASC),
    FOREIGN KEY (PositionId) REFERENCES Positions (PositionId),
    FOREIGN KEY (EmpId) REFERENCES Employee (EmpId)
);

GO
 CREATE proc CompanyAddOrEdit
 @EmpCmpId int ,
 @EmpCmpName varchar(50),
 @positionId int,
 @EmpId int,
@YearOfExp int
 As
 if @EmpCmpId = 0
 insert into Company(EmpCmpName,PositionId,EmpId,YearOfExp) values(@EmpCmpName,@PositionId,@EmpId,@YearOfExp)

 else 
 update Company
 set
  EmpCmpName=@EmpCmpName,
  PositionId=@positionId,
  EmpId=@EmpId,
  YearOfExp=@YearOfExp
  where EmpCmpId=@EmpCmpId
  go

 create proc CompanyDelete
 @EmpEmpId int 
 as
 delete from Company
 where EmpCmpId=@EmpEmpId
 go

 create proc EmployeeAddOrEdit
 @EmpId int ,
 @EmpCod varchar(30),
 @EmpName varchar(50),
 @positionId int,
 @DOB date,
 @Gender varchar(20),
 @Status varchar(15),
 @ImagePath nvarchar(max)
 As
 if @EmpId = 0 begin
 insert into Employee(EmpCod,EmpName,PositionId,DOB,Gender,Status,imagePath) values(@EmpCod,@EmpName,@PositionId,@DOB,@Gender,@Status,@imagePath)

 select SCOPE_IDENTITY();
 end
 else begin update Employee
 set
 EmpCod=@EmpCod,
 EmpName=@EmpName,
 PositionId=@positionId,
 DOB=@DOB,
 Gender=@Gender,
 Status=@Status,
 imagePath=@ImagePath
 where EmpId=@EmpId
  select @EmpId;
  end
go

 create proc EmployeeDelete
 @EmpId int 
 as
 delete from Employee
 where EmpId=@EmpId

  delete from Company
 where EmpId=@EmpId

 go

  create proc EmployeeViewAll
 as
select E.EmpId,E.EmpCod,E.EmpName,P.Position,E.Status,E.DOB
from Employee E inner Join Positions P on E.PositionId= P.PositionId
go
 create proc EmployeeViewById
 @EmpId int 
 as
 select * from Employee
 where EmpId=@EmpId

 select * from Company
 where EmpId=@EmpId