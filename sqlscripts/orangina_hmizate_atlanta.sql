-- ORANGINA
select count(*) from T_FB_POST where fk_influencer = '1033839970079371'
select count(*) from FBFeedComments C INNER JOIN T_FB_POST P ON C.feedId = P.id and P.fk_influencer = '1033839970079371'

--- ATLANTA
select count(*) from T_FB_POST where fk_influencer = '735919043138496'
select count(*) from FBFeedComments C INNER JOIN T_FB_POST P ON C.feedId = P.id and P.fk_influencer = '735919043138496'

--- HMIZATE
select count(*) from T_FB_POST where fk_influencer = '238920879546038'
select count(*) from FBFeedComments C INNER JOIN T_FB_POST P ON C.feedId = P.id and P.fk_influencer = '238920879546038'

-- select * from T_FB_INFLUENCER 