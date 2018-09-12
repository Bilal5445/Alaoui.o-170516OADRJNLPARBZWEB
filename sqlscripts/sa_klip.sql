-- select * from T_XTRCTTHEME_KEYWORD

/*select Keyword_Type, sum(keyword_count) COUNT from T_XTRCTTHEME_KEYWORD
group by Keyword_Type
order by sum(keyword_count) desc*/

/*select top 10 Keyword, sum(keyword_count) COUNT from T_XTRCTTHEME_KEYWORD
group by Keyword
order by sum(keyword_count) desc*/

select Keyword_Type, sum(keyword_count) COUNT from T_XTRCTTHEME_KEYWORD
where keyword_type = 'NEGATIVE' OR keyword_type = 'POSITIVE' OR keyword_type = 'EXPLETIVE' or keyword_type = 'SUPPORT' or keyword_type = 'SENSITIVE' or keyword_type = 'OPPOSE'
group by Keyword_Type
order by sum(keyword_count) desc