select I.name FB_Page, count(*) NB_Comments from FBFeedComments C
INNER JOIN T_FB_POST P ON C.feedId = P.id
INNER JOIN T_FB_INFLUENCER I ON P.fk_influencer = I.id
group by I.name
ORDER BY count(*) DESC

select count(*) from FBFeedComments
where feedId is null