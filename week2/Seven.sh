#!/bin/bash

stops=$(tr ',' '\n' < ../stop_words.txt; echo {a..z})
words=$(tr -sc 'A-Za-z' '\n' < "$1" | tr A-Z a-z | grep -vFx -f <(echo "$stops"))
echo "$words" | sort | uniq -c | sort -nr | head -n 25

