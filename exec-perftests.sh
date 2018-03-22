#!/bin/bash
runs=$1
for (( run=0; run<$runs; run++ ))
do
  mkdir "perf-test-$run"
  for (( i=0; i<=3; i++ ))
  do
    size=$((10**($i+2)))
    for (( j=0; j<=9; j++ ))
    do
      factor=$((2**$j))
      number=$((10+$i*10+$j))
      client_buffer=$(($factor*40))
      buff_size=$(($factor*500))
      live_buff_size=$(($factor*500))
      read_batch=$(($factor*20))
      echo "Experiment number $number of run $run, size $size"
      # echo "Performance test no. $number for size=$size, client buffer=$client_buffer, buffer size=$buff_size, live buffer size=$live_buff_size, read batch=$read_batch"
      source ./performance.sh $size $client_buffer $buff_size $live_buff_size $read_batch $number $run
    done
  done
done
