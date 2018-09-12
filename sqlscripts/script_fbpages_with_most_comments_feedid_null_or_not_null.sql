select FB_Page, sum(NB_Comments) NBs_Comments 
from (
	select I.name FB_Page, count(*) NB_Comments from FBFeedComments C
	INNER JOIN T_FB_POST P ON C.feedId = P.id
	INNER JOIN T_FB_INFLUENCER I ON P.fk_influencer = I.id
	group by I.name

	UNION

	select I.name FB_Page, count(*) NB_Comments from FBFeedComments C
	INNER JOIN T_FB_POST P ON LEFT(C.id, CHARINDEX('_', C.id) - 1) = RIGHT(P.id, CHARINDEX('_', P.id) - 1)
	INNER JOIN T_FB_INFLUENCER I ON P.fk_influencer = I.id
	where C.feedId is null
	AND CHARINDEX('_', C.id) > 0
	AND CHARINDEX('_', P.id) > 0
	group by I.name
) A
GROUP BY FB_Page
ORDER BY sum(NB_Comments) DESC

