@echo on 
echo "1/4 --- Build and create Image"
CALL docker build -t  flowmodoro:latest .
CALL docker save -o ./Server/bin/flowmodoro.tar flowmodoro:latest

echo "2/4 --- Copy to test environment"
pscp -P 22 -pw %1 "D:\Documents\_Mein Stuff\ProgrammingStuff\rando\Flowmodoro\Server\bin\flowmodoro.tar" pi@192.168.0.122:/home/pi/Images

echo "ssh to test env and install"
START /WAIT putty.exe pi@192.168.0.122:22 -pw %1 -m ./remoteMachineCommands.txt -t
echo "Done!"