#!/bin/bash
stopwords=$(tr ',' '\n' < ../stop_words.txt)
words=$(grep -oE '[a-zA-Z]{2,}' "$1" | tr 'A-Z' 'a-z' | grep -vFx -f <(echo "$stopwords"))
echo "$words" | sort | uniq -c | sort -nr | head -n 25 | awk '{print $2 " - " $1}'
