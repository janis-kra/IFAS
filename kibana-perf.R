library(ggplot2)

setwd(".")

caching.path <- "kibana-perf_with-caching.csv"
caching.file <- read.csv(caching.path, header = TRUE, sep = ";", quote = "\"", dec = ".", fill = TRUE, comment.char = "")
nocaching.path <- "kibana-perf_without-caching.csv"
nocaching.file <- read.csv(nocaching.path, header = TRUE, sep = ";", quote = "\"", dec = ".", fill = TRUE, comment.char = "")

#data from Index_Time-1522440318686.json via localhost:9200/performance-120mio_experimentparticipatedevent/_stats/indexing?pretty

caching.mean.query_duration <- mean(caching.file$query_duration)
caching.mean.request_duration <- mean(caching.file$request_duration)
caching.mean.hits <- mean(caching.file$hits)
nocaching.mean.query_duration <- mean(nocaching.file$query_duration)
nocaching.mean.request_duration <- mean(nocaching.file$request_duration)
nocaching.mean.hits <- mean(nocaching.file$hits)

speedup.query_duration <- nocaching.mean.query_duration / caching.mean.query_duration
speedup.request_duration <- nocaching.mean.request_duration / caching.mean.request_duration
