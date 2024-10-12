#!/bin/bash

stops=$(tr ',' '\n' < ../stop_words.txt; echo {a..z})
words=$(tr -sc 'A-Za-z' '\n' < "$1" | tr A-Z a-z | grep -vFx -f <(echo "$stops"))
sorted_words=$(echo "$words" | sort | uniq -c | sort -nr | head -n 25)

echo "$sorted_words" | awk '{print $2 " - " $1}'

