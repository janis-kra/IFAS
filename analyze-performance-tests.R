library(ggplot2)

setwd(".")

# constants etc.
sizes <- 3
steps <- 10
runs <- 1 # at least 1 run - overwritten later
width <- 20 #cm
height <- 14 #cm

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
    scale_x_continuous(name="Time (s)", breaks = seq(0, x.limit, x.intervals), limits = c(0, x.limit)) +
    scale_y_continuous(name = "Events", limits = c(0, 8200)) +
    geom_smooth(color = 'red', se = FALSE, span = 0.4) +
    geom_line()
  # todo: use which ncol value for face_wrap? 2 seems good
}

plotTotalDuration <- function (d, breaks) {
  ggplot(d, aes(x=config, y=t, group = config)) +
    geom_boxplot(outlier.shape = 3) +
    scale_x_continuous(name = "Configuration", breaks = c(0:10)) +
    scale_y_log10(name = "Duration (s)", breaks = breaks)
  # todo konfidenzintervalle anpassen?
  # todo mehr experimente
}

results.singlerun <- subset(results.flat, run == 10)

plotAmount(subset(results.singlerun, size == 50000), 15, 3)
ggsave("plots/config-comparison_50k.pdf", device = "pdf", width = width, height = height, units = "cm")

plotAmount(subset(results.singlerun, size == 100000), x.limit = 90, x.intervals = 10)
ggsave("plots/config-comparison_100k.pdf", device = "pdf", width = width, height = height, units = "cm")

plotAmount(subset(results.singlerun, size == 150000), 200, 30)
ggsave("plots/config-comparison_150k.pdf", device = "pdf", width = width, height = height, units = "cm")

results.durations.50k <- subset(results.durations, size == 50000)
plotTotalDuration(results.durations.50k, c(5,10,15,20,25,30,40,50,60,80,100,150,200,300,500))
ggsave("plots/mean-durations-50k.pdf", device = "pdf", width = width, height = 10, units = "cm")

results.durations.100k <- subset(results.durations, size == 100000)
plotTotalDuration(results.durations.100k, c(10,20,30,40,50,75,100,125,150,200,300,400,500,750,1000))
ggsave("plots/mean-durations-100k.pdf", device = "pdf", width = width, height = 10, units = "cm")

results.durations.150k <- subset(results.durations, size == 150000)
plotTotalDuration(results.durations.150k, c(10,20,30,40,50,75,100,125,150,200,300,400,500,750,1000,1500))
ggsave("plots/mean-durations-150k.pdf", device = "pdf", width = width, height = 10, units = "cm")

results.durations.50k.config8.median <- median(subset(results.durations.50k, config == 8)$t)
results.durations.100k.config8.median <- median(subset(results.durations.100k, config == 8)$t)
results.durations.150k.config8.median <- median(subset(results.durations.150k, config == 8)$t)
