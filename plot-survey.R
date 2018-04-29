library(ggplot2)
library(reshape2)

setwd("/Users/jk/Documents/Uni/master/impl/eek/plots")

multiplot <- function(..., plotlist=NULL, file, cols=1, layout=NULL) {
  library(grid)
  
  # Make a list from the ... arguments and plotlist
  plots <- c(list(...), plotlist)
  
  numPlots = length(plots)
  
  # If layout is NULL, then use 'cols' to determine layout
  if (is.null(layout)) {
    # Make the panel
    # ncol: Number of columns of plots
    # nrow: Number of rows needed, calculated from # of cols
    layout <- matrix(seq(1, cols * ceiling(numPlots/cols)),
                     ncol = cols, nrow = ceiling(numPlots/cols))
  }
  
  if (numPlots==1) {
    print(plots[[1]])
    
  } else {
    # Set up the page
    grid.newpage()
    pushViewport(viewport(layout = grid.layout(nrow(layout), ncol(layout))))
    
    # Make each plot, in the correct location
    for (i in 1:numPlots) {
      # Get the i,j matrix positions of the regions that contain this subplot
      matchidx <- as.data.frame(which(layout == i, arr.ind = TRUE))
      
      print(plots[[i]], vp = viewport(layout.pos.row = matchidx$row,
                                      layout.pos.col = matchidx$col))
    }
  }
}
perc <- function (x,y) {
  (x/y)*100
}

fullCbbPalette <- c("#000000", "#E69F00", "#56B4E9", "#009E73", "#F0E442", "#0072B2", "#D55E00", "#CC79A7", "#999999", "#E69F00", "#56B4E9", "#009E73")
cbbPalette <- c("#009E73", "#0072B2")

f <- c("Yes","No")
survey.q1 <- data.frame(
  x = factor(f, levels=f),
  y = c(58.3, 41.7)
)
f <- c("Dogs","Cats")
survey.q2 <- data.frame(
  x = factor(f, levels=f),
  y = c(75, 25)
)
f <- c("Channel Switcher","Menu")
survey.q3 <- data.frame(
  x = factor(f, levels=f),
  y = c(50, 50)
)
survey.q4 <- data.frame(
  x = seq(1,12),
  y = c(2,2,1,1,1,3,1,1,2,1,1,1)
)
f <- c("don't know","blue","green")
survey.q5 <- data.frame(
  x = factor(f, levels=f),
  y = c(perc(8,12),perc(3,12),perc(1,12))
)

plotSettings <- function (palette, t) {
  
}
plotAnswer <- function (d, t, palette = cbbPalette) {
  ggplot(data=d, aes(x=x, y=y, fill=x)) +
    geom_bar(colour="black", width=.8, stat="identity") +
    xlab("Answer") +
    ylab("Percentage") +
    ylim(c(0, 100)) +
    scale_fill_manual(values=palette) +
    ggtitle(t) +
    guides(fill=FALSE) +
    coord_flip()
}

survey.q1.results <- plotAnswer(survey.q1,"Did you participate in the tutorial?")
survey.q2.results <- plotAnswer(survey.q2,"Which was the more interesting channel?")
survey.q3.results <- plotAnswer(survey.q3,"Which method for switching channels did you use more?")
survey.q4.results <- ggplot(data=survey.q4, aes(x=x, y=y, fill=x)) +
  geom_bar(colour="black", width=.8, fill="#999999", stat="identity") +
  xlab("User") +
  ylab("Amount of messages") +
  ylim(c(0, 4)) +
  scale_fill_manual(values=fullCbbPalette[9]) +
  ggtitle("How many messages did you send?") +
  guides(fill=FALSE)
survey.q5.results <- plotAnswer(survey.q5,"What color did the button for starting the tutorial have? Click \"don't know\" if you do not remember.",fullCbbPalette[c(9,6,4)])

pdf("survey.pdf", 10, 14)
multiplot(survey.q1.results, survey.q2.results, survey.q3.results, survey.q5.results, survey.q4.results, cols = 1)
dev.off()

survey.q4.results.mean <- mean(survey.q4$y)

# f <- c("Dogs", "Cats")
# kibana.channelInterest.messages <- data.frame(
#   x = factor(f, levels=f),
#   y = c(47, 21)
# )
# kibana.channelInterest.scrolling <- data.frame(
#   x = factor(f, levels=f),
#   y = c(62, 25)
# )
# 
# kibana.tutorialButton <- data.frame(
#   groups = c("Treatment", "Control"),
#   started = c((4/6)*100,(3/6)*100),
#   skipped = c((2/6)*100,(3/6)*100)
# )
# ggplot(
#   data=melt(kibana.tutorialButton, id.var="groups"),
#   aes(x=groups, y= value, fill=variable)
# ) +
#   geom_bar(colour="black", width=.8, stat="identity") +
#   xlab("Answer") +
#   ylab("Percentage") +
#   #ylim(c(0, 100)) +
#   scale_fill_manual(values=cbbPalette,name="Decision",
#                     labels=c("Started", "Skipped")) +
#   ggtitle("A/B Test Results: Tutorial Button Styling")
# 
# kibana.channelSwitching <- data.frame(
#   group = c("Quick Switcher", "Menu"),
#   value = c(perc(20,43),perc(23,43))
# )
# ggplot(kibana.channelSwitching, aes(x="", y=value, fill=group)) +
#   scale_fill_manual(values=cbbPalette,
#                     name="Decision") +
#   geom_bar(width = 1, stat = "identity")+
#   coord_polar("y", start=0)
