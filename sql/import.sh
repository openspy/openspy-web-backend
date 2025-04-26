ls /sql
echo "pass " $MYSQL_PASSWORD
echo "host" $MYSQL_HOST
sleep 10
mysql -u $MYSQL_USER --password=$MYSQL_PASSWORD -h $MYSQL_HOST -e 'create database Gamemaster;'
mysql -u $MYSQL_USER --password=$MYSQL_PASSWORD -h $MYSQL_HOST --database=Gamemaster < /sql/Gamemaster.sql

mysql -u $MYSQL_USER --password=$MYSQL_PASSWORD -h $MYSQL_HOST -e 'create database GameTracker;'
mysql -u $MYSQL_USER --password=$MYSQL_PASSWORD -h $MYSQL_HOST --database=GameTracker < /sql/GameTracker.sql

mysql -u $MYSQL_USER --password=$MYSQL_PASSWORD -h $MYSQL_HOST -e 'create database KeyMaster;'
mysql -u $MYSQL_USER --password=$MYSQL_PASSWORD -h $MYSQL_HOST --database=KeyMaster < /sql/KeyMaster.sql

mysql -u $MYSQL_USER --password=$MYSQL_PASSWORD -h $MYSQL_HOST -e 'create database Peerchat;'
mysql -u $MYSQL_USER --password=$MYSQL_PASSWORD -h $MYSQL_HOST --database=Peerchat < /sql/Peerchat.sql

mysql -u $MYSQL_USER --password=$MYSQL_PASSWORD -h $MYSQL_HOST --database=GameTracker < /sql/user_seed.sql

echo 'seed rabbitmq'
python3 /sql/rabbitmqadmin declare exchange -H $RABBITMQ_HOST --user=$RABBITMQ_DEFAULT_USER --password=$RABBITMQ_DEFAULT_PASS --vhost=$RABBITMQ_DEFAULT_VHOST name=openspy.core type=topic durable=true
python3 /sql/rabbitmqadmin declare exchange -H $RABBITMQ_HOST --user=$RABBITMQ_DEFAULT_USER --password=$RABBITMQ_DEFAULT_PASS --vhost=$RABBITMQ_DEFAULT_VHOST name=openspy.master type=topic durable=true
python3 /sql/rabbitmqadmin declare exchange -H $RABBITMQ_HOST --user=$RABBITMQ_DEFAULT_USER --password=$RABBITMQ_DEFAULT_PASS --vhost=$RABBITMQ_DEFAULT_VHOST name=openspy.natneg type=topic durable=true
python3 /sql/rabbitmqadmin declare exchange -H $RABBITMQ_HOST --user=$RABBITMQ_DEFAULT_USER --password=$RABBITMQ_DEFAULT_PASS --vhost=$RABBITMQ_DEFAULT_VHOST name=openspy.gamestats type=topic durable=true
python3 /sql/rabbitmqadmin declare exchange -H $RABBITMQ_HOST --user=$RABBITMQ_DEFAULT_USER --password=$RABBITMQ_DEFAULT_PASS --vhost=$RABBITMQ_DEFAULT_VHOST name=presence.core type=topic durable=true


curl -v -X POST "http://$HTTP_API_URL/v1/Game/SyncToRedis" -H  "accept: application/json" -H  "APIKey: $HTTP_API_KEY"
curl -v -X POST "http://$HTTP_API_URL/v1/Group/SyncToRedis" -H  "accept: application/json" -H  "APIKey: $HTTP_API_KEY"