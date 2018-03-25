library(ggplot2)

setwd("/Users/jk/Documents/Uni/master/impl/eek")

# constants etc.
sizes <- 4
steps <- 10
runs <- 3

# functions

getExpNumber <- function (f) {
  pos <- regexpr("[0-9]+", f)[1]
  as.numeric(substr(f, pos, pos+1)) - 10
}

getRunNumber <- function (f) {
  pos <- regexpr("[0-9]", f)[1]
  as.numeric(substr(f, pos, nchar(f)))
}

readFile <- function (file, folder) {
  # todo: calc mean over all experiment runs!
  expNumber <- getExpNumber(file) # zero based!
  runNumber <- getRunNumber(folder)
  configNumber <- expNumber %% steps # zero based!
  sizeNumber <- floor(expNumber / steps)
  size <- 10^(sizeNumber+2)
  path <- paste(folder, file, sep = "/")
  data <- read.csv(path, header = TRUE, sep = ";", quote = "\"", dec = ".", fill = TRUE, comment.char = "")
  names(data) <- c("amount", "totalAmount", "timestamp", "comment")
  data <- head(data,-1)
  minimum <- min(data$timestamp)
  transform(
    data,
    t = (timestamp - minimum)/1000,
    size = size,
    config = configNumber,
    run = runNumber
  )
}

# program
results.folders <- list.files(pattern="perf-test-*", include.dirs = TRUE)
results <- lapply(results.folders, function (folder) {
  lapply(list.files(path=folder, pattern="*.csv"), function (file) {
    readFile(file, folder)
  })
})
results.unlisted <- unlist(results, recursive = FALSE)
results.flat <- do.call("rbind", results.unlisted)
results.durations <- do.call("rbind", lapply(results.unlisted, function (df) {
  tail(df, 1)
}))

results.firstRun <- results[[1]]

e100k.list <- results.firstRun[c(31:40)]
e_100 <- do.call(rbind, results.firstRun[c(1:10)])
e_1000 <- do.call(rbind, results.firstRun[c(11:20)])
e_10000 <- do.call(rbind, results.firstRun[c(21:30)])
e100k.data <- do.call(rbind, e100k.list)
e_200000 <- do.call(rbind, results.firstRun[c(31:40)])

plotAmount <- function (d) {
  ggplot(d, aes(x = d$t, y = d$amount)) +
    geom_line() +
    facet_wrap(~config, ncol=2) +
    geom_smooth(method = 'loess', color = 'red', se = FALSE, span = 0.4)
  # todo: use log scale? maybe not?
  # todo: use which ncol value for face_wrap?
  # todo: legend
}

plotTotalDuration <- function (d) {
  ggplot(d, aes(x=config, y=t, group = config)) + geom_boxplot()
  # todo legend
}

results.run0 <- subset(results.flat, run == 0)
plotAmount(subset(results.run0, size == 100000))
ggsave("config-comparison_100k.pdf", device = "pdf")

# durations <- vapply(results.flat, function (df) {
#   tail(df,1)$t
# }, FUN.VALUE = 1)
# durations.mean <- vector(length = length(results[[1]]))
# for (i in 1:length(durations.mean)) {
#   durations.mean[i] <- mean(vapply(results, function (run) {
#     tail(run[[i]], 1)$t
#   }, FUN.VALUE = 1))
# }
# configs <- c(1:steps)
# e100k.durationsPerConfig <- data.frame(duration = durations.mean[c(31:40)], configs)
plotTotalDuration(subset(results.durations, size == 100000))
ggsave("mean-durations-100k.pdf", device = "pdf")
