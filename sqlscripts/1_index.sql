/*SELECT P.* FROM T_FB_POST P 
                            WHERE ( 
                                (P.post_text LIKE N'%" + filter + "%' OR P.translated_text LIKE N'%" + filter + "%') 
                                OR 
                                P.id IN ( 
                                    SELECT C.feedId FROM FBFeedComments C WHERE C.message LIKE N'%" + filter + "%' OR C.translated_message LIKE N'%" + filter + "%' 
                                ) 
                            ) 
                            ORDER BY P.date_publishing DESC*/

/*SELECT C.feedId FROM FBFeedComments C WHERE C.message LIKE N'%لمستفيد الوحيد%' 
OR C.translated_message LIKE N'%لمستفيد الوحيد%' */

/*UPDATE FBFeedComments SET message = LEFT(message, 1700) WHERE LEN(message) > 1700
UPDATE FBFeedComments SET translated_message = LEFT(translated_message, 1700) WHERE LEN(translated_message) > 1700*/

-- ALTER table FBFeedComments REBUILD

/*UPDATE FBFeedComments SET message = LEFT(message, 849) WHERE LEN(message) > 849
UPDATE FBFeedComments SET translated_message = LEFT(translated_message, 849) WHERE LEN(message) > 849*/

ALTER TABLE FBFeedComments
ALTER COLUMN message nvarchar(849);