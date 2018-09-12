select I.name, count(*) from T_FB_POST P
INNER JOIN T_FB_INFLUENCER I ON P.fk_influencer = I.id
group by I.name