library(ggplot2)

setwd(".")

#path <- "120mio-shards-indexing-time.csv"
#file <- read.csv(path, header = TRUE, sep = ";", quote = "\"", dec = ".", fill = TRUE, comment.char = "")

#data from Index_Time-1522440318686.json via localhost:9200/performance-120mio_experimentparticipatedevent/_stats/indexing?pretty

index_total <- 120000000
index_time_in_millis <- 7456591
index_time_in_seconds <- index_time_in_millis / 1000
index_time_in_minutes <- index_time_in_seconds / 60

indexing_rate <- index_total / index_time_in_seconds
