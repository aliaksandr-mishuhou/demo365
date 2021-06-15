REM build new ones
docker build -t demo365.api -f Api.Dockerfile .
docker build -t demo365.fakesource -f FakeSource.Dockerfile .
docker build -t demo365.repository -f Repository.Dockerfile .
docker build -t demo365.repository.db -f Repository.DB.Dockerfile .
