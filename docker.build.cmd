REM cleanup
REM docker rmi -f demo365.api
REM docker rmi -f demo365.fakesource
REM docker rmi -f demo365.repository 
REM // docker rmi -f demo365.repository.db 

REM build new ones
docker build -t demo365.api -f Api.Dockerfile .
docker build -t demo365.fakesource -f FakeSource.Dockerfile .
docker build -t demo365.repository -f Repository.Dockerfile .
REM // docker build -t demo365.repository.db -f Repository.DB.Dockerfile .
