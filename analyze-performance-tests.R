library(ggplot2)

setwd("/Users/jk/Documents/Uni/master/impl/eek")

# constants etc.
sizes <- 3
steps <- 10
runs <- 1 # at least 1 run - overwritten later

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
  size <- (sizeNumber+1) * 50000
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
runs <- length(results)
results.unlisted <- unlist(results, recursive = FALSE)
results.flat <- do.call("rbind", results.unlisted)
results.durations <- do.call("rbind", lapply(results.unlisted, function (df) {
  tail(df, 1)
}))

plotAmount <- function (d, x.limit, x.intervals) {
  ggplot(d, aes(x = d$t, y = d$amount)) +
    #scale_y_log10() + # log scale not good because fluctuations at < 1000 events are not interesting
    facet_wrap(~config, ncol=2) +
    scale_x_continuous(name="Time (s)", breaks = seq(0, 100, x.intervals), limits = c(0, x.limit)) +
    scale_y_continuous(name = "Events") +
    geom_smooth(method = 'loess', color = 'red', se = FALSE, span = 0.4) +
    geom_line()
  # todo: use which ncol value for face_wrap? 2 seems good
}

plotTotalDuration <- function (d) {
  ggplot(d, aes(x=config, y=t, group = config)) +
    geom_boxplot() +
    scale_x_continuous(name = "Configuration", breaks = c(0:10)) +
    scale_y_log10(name = "Duration (s)", breaks = seq(0, 100, 10))
  # todo konfidenzintervalle anpassen?
  # todo mehr experimente
}

results.run0 <- subset(results.flat, run == 0)

plotAmount(subset(results.run0, size == 100000), x.limit = 100, x.intervals = 10)
ggsave("config-comparison_100k.pdf", device = "pdf")

plotAmount(subset(results.run0, size == 50000), 25, 5)

plotTotalDuration(subset(results.durations, size == 100000))
ggsave("mean-durations-100k.pdf", device = "pdf")
plotTotalDuration(subset(results.durations, size == 10000)) +
  scale_y_log10(name = "Duration (s)", breaks = seq(0, 5, 1))
#plotTotalDuration(subset(results.durations, size == 200000))
#ggsave("mean-durations-200k.pdf", device = "pdf")
