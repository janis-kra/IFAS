#!/bin/bash
for i in 100 1000 10000 100000
do
  size=$i
  echo "$size"
  for (( j=0; j<=7; j++ ))
  do
    factor=$((2**$j))
    client_buffer=$(($factor*40))
    buff_size=$(($factor*500))
    live_buff_size=$(($factor*500))
    read_batch=$(($factor*20))
    echo "Performance test for size=$size, client buffer=$client_buffer, buffer size=$buff_size, live buffer size=$live_buff_size, read batch=$read_batch"
    source ./performance.sh $size $client_buffer $buff_size $live_buff_size $read_batch
  done
done
