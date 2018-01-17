﻿CREATE TABLE [dbo].[ClientKeys] (
    [ClientKeysID] [int] NOT NULL IDENTITY,
    [RegisterAppId] [int],
    [ClientId] [nvarchar](max),
    [ClientSecret] [nvarchar](max),
    [CreatedOn] [datetime],
    [UserID] [nvarchar](max),
    CONSTRAINT [PK_dbo.ClientKeys] PRIMARY KEY ([ClientKeysID])
)
CREATE INDEX [IX_RegisterAppId] ON [dbo].[ClientKeys]([RegisterAppId])
CREATE TABLE [dbo].[RegisterApps] (
    [RegisterAppId] [int] NOT NULL IDENTITY,
    [Name] [nvarchar](max) NOT NULL,
    [TotalAppCallLimit] [int] NOT NULL,
    [TotalAppCallConsumed] [int] NOT NULL,
    [CreatedOn] [datetime],
    [UserID] [nvarchar](max),
    CONSTRAINT [PK_dbo.RegisterApps] PRIMARY KEY ([RegisterAppId])
)
CREATE TABLE [dbo].[RegisterAppCallingLogs] (
    [RegisterAppCallingLogId] [int] NOT NULL IDENTITY,
    [UserId] [nvarchar](max),
    [RegisterAppId] [int],
    [TokensManagerID] [int],
    [DateCreatedOn] [datetime],
    [MethodName] [nvarchar](max),
    CONSTRAINT [PK_dbo.RegisterAppCallingLogs] PRIMARY KEY ([RegisterAppCallingLogId])
)
CREATE INDEX [IX_RegisterAppId] ON [dbo].[RegisterAppCallingLogs]([RegisterAppId])
CREATE INDEX [IX_TokensManagerID] ON [dbo].[RegisterAppCallingLogs]([TokensManagerID])
CREATE TABLE [dbo].[TokensManagers] (
    [TokensManagerID] [int] NOT NULL IDENTITY,
    [TokenKey] [nvarchar](max),
    [IssuedOn] [datetime],
    [ExpiresOn] [datetime],
    [CreaatedOn] [datetime],
    [RegisterAppId] [int],
    [IsDeleted] [bit] NOT NULL,
    CONSTRAINT [PK_dbo.TokensManagers] PRIMARY KEY ([TokensManagerID])
)
CREATE INDEX [IX_RegisterAppId] ON [dbo].[TokensManagers]([RegisterAppId])
CREATE TABLE [dbo].[RegisterUser] (
    [UserID] [int] NOT NULL IDENTITY,
    [Username] [nvarchar](30) NOT NULL,
    [Password] [nvarchar](30) NOT NULL,
    [CreateOn] [datetime] NOT NULL,
    [EmailID] [nvarchar](max) NOT NULL,
    CONSTRAINT [PK_dbo.RegisterUser] PRIMARY KEY ([UserID])
)
ALTER TABLE [dbo].[ClientKeys] ADD CONSTRAINT [FK_dbo.ClientKeys_dbo.RegisterApps_RegisterAppId] FOREIGN KEY ([RegisterAppId]) REFERENCES [dbo].[RegisterApps] ([RegisterAppId])
ALTER TABLE [dbo].[RegisterAppCallingLogs] ADD CONSTRAINT [FK_dbo.RegisterAppCallingLogs_dbo.RegisterApps_RegisterAppId] FOREIGN KEY ([RegisterAppId]) REFERENCES [dbo].[RegisterApps] ([RegisterAppId])
ALTER TABLE [dbo].[RegisterAppCallingLogs] ADD CONSTRAINT [FK_dbo.RegisterAppCallingLogs_dbo.TokensManagers_TokensManagerID] FOREIGN KEY ([TokensManagerID]) REFERENCES [dbo].[TokensManagers] ([TokensManagerID])
ALTER TABLE [dbo].[TokensManagers] ADD CONSTRAINT [FK_dbo.TokensManagers_dbo.RegisterApps_RegisterAppId] FOREIGN KEY ([RegisterAppId]) REFERENCES [dbo].[RegisterApps] ([RegisterAppId])
INSERT [dbo].[__MigrationHistory]([MigrationId], [ContextKey], [Model], [ProductVersion])
VALUES (N'201711241555349_TokenAuthentication', N'ArabicTextAnalyzer.Models.ArabiziDbContext',  0x1F8B0800000000000400ED5DD96EE3C8157D0F907F20F894043D92DD1D0413C39E815A52F728E30D967ACB8B5196CA32132E1A2E1DBB07F9B23CE493F20B299222591B6B6351A6836080418BAC3AB59D7B59DB3DFECFBFFE7DFAE363E03B5F619C785178E61E8F8E5C0786EB68E385DB33374BEFBFFBDEFDF187DFFEE674BE091E9D8F55BA37793A94334CCEDC8734DD9D8CC7C9FA0106201905DE3A8E92E83E1DADA3600C36D1F8F5D1D19FC7C7C76388205C84E538A73759987A012C7EA09FD3285CC35D9A01FF22DA403FD93F476F9605AA73090298ECC01A9EB99318DC79EB157C4C2721F09FBEC1785466729D89EF0154A125F4EF5D07846194821455F7E4430297691C85DBE50E3D00FEEA690751BA7BE02770DF8C9326B96A8B8E5EE72D1A37192BA87596A451A00978FC66DF45633ABB5147BB751716DD1BEC7CF89837BBE8C93337EFBE391A82F4C975E8F24EA67E9C27E576F50CA5F4C25193FF95D39AEA55CD1744ABFCBF57CE34F3D32C866721CCD218F8AF9CEBECCEF7D63FC3A755F477189E8599EFE3554795BF8EA31D8CD3A77DCDA711A28EEB94155C84E99FFEE83A972817B8F3613DA463214459F1C5A64241DC406C779D0BF0780EC36DFA70E6A27FBACE3BEF116EAA277BE40FA1878C03654AE34C5AD0050CCB01ECB99CCB280E80EF7D83FD372987B75CC8E918E327FEBC1C278CB553DF433D8AC89298B1B6F413A306A677F2A277C403AC436FE03DD3ACC58CE9FF310DC1180491BBB68B37AF39768175D7328D62F81E86300629DC5C83348531F2548B0DDCBB05310F6EE0D64B5096C96ED718D2BE5871CEB2BE07B0BEB2A0255CC730EDBFB018E6FD78551BFB0CFD5CA14F9C2C23FA36C5CDC0D9ABDF25F8EA6D8BA16E1F39644637D02F12250FDEAEFC7262E6714BA67D1747C14DE4139C2392DC2EA32C5EE7FE2112A75B81789B0F0AE9061A7B177A010CA9931BC07086E107289BD275042293ECD113E4FFB7405FCD6FF80AD5DE472D9D02DF3FF7022F9535581D0E4D46932C80D22E7C490E41D9B82E6E273793B78BE96C72B3F8CB647EB9BAF9727B3E592D2E3F5DDDCC3A999B10791806B898895BAF6B9132BCB2E7DE67DE469B5E3CE84E80E7683CC34F51DCFF77F923883D10A6093D933730B28B2849AFA35DE683788FDAFF04380661E2831EE6F59D6C7435FFBC42FF5AACBE5837D2067AC0568AB7DF8A99E28043B2537CE5BEA75F3E69C31F0B11BBB0CC36B586CB273B24B2C29CBF2EBA6395DD3C432EF26F20EFEC67F65C7593BAD3690F352C26D5ED332591B531FFE6F530DCAD65A1F18E9FF2792E3BE7B5E593569F1697EFCFBF4CA6D3AB0F97AB8E0C22C106C321B25AB793EBC5EDCF733336B54175E055BE3EB1B4E61317B4466BB1E4F63E86B0E34A2C8B63B4949DAC53EFABFDBD4B65E67E5EDD4C57AB9FE617F37C082C2CA758C0C13098D75603F6F260BA7D511BC44E48A8EEFFB0BF5432229335120D903C1D49D36D9EFD000378103F3708FF846D1EE67B60A8ECF3686B6B7BB5411C06C9B855EBB8E54A021D6AF3B5D82CEC7FCBC6FCB4A718A8E40284608B6F6B2AE5CD678FC6DBA817307D8836FD18B0F9D90A9730ADC72CF2D4CC898B4216DEE18BA851C410EA348BCE286B18995EB16954A64E274B045627E747200DC3E93196A8EBECC4A6DCA3932B0A46B5EBDDCD2D922433F135F3C79D17C3443F63EEDDCCDC9BB9475E2433E8C3B439DF7A1B21F302A17C8664EEF608EAB4BABBF6548C2F1024B572B69C7F57ADCC7E72A061D87F75AEA86BF6E479E461A634A1F8B3FDE6A88F23E56B902492555D3F0597731C9E0FD0BDD88628E55B393B36DAAE9B2449B4F60A1230779348A3275B310F378EE2BD92E6C003BF0B76814CC6DB212341953A73FFC07493BC80CA69600510D74CC8128E46A363A6106469305FC779A0B841806CD70B53D62CBD70EDED80AF561F2ABBFEAD917CB0EA32E93733B883616E956ABD6FA5327599941792F5DEE918639798740A73EC367AE84CB8B94CC1D7CEEAACD498B41F9AA0EA553B2C57D547EAA5D05630476AE38DCA84093B22265737EAF454986C1D9A96F22A1D968EF291782934545ACAEB39B2B6757DEF1EB4656F40D526FAF6A2FCEAA91045B296B7E049F96366A96ED6695CCE47519E14E58031792A3DBBCB5F14C7E0CC820E2D36F66BBA643FF5A51996032F61CA4C685DA7990473A6A40C53491C7209CC20113E540225BC57C903975CF13428AEB92BA55A9EE0BA9642818AA5A842EF6F5B0850DB2E74B080E4813B1F92BEC3200565CF41F9C00AC7AE227019A80EAB1B5722E137EEEF25F094B36650A9F78A95CDF71B44752CB7822830CC0FF19C03B5C585A596C458D0AE527D695C378F74528CF3555F0B63886473E81D24B233143A4AE51484ED33DD759DE9CA8EDF6E82D0825ED558CBF5D7C1A2FD56B66355571EBA6B0FAC7D42D3D45D6C1C9A98F439962A35459366F369731FF46C9928AB0EA0A0A7ABBDC27A6E56BF3B1D97B1DDFB077914283708FCF402EC76A89E5850F8FE89B32C23C2A7DF2DF563A4831263BC2606869E49D625A5518C5A4EBDCDA39137F09D1727E90CA4E00EE43BA5D34DC0246366A22D9FA6AA3866B2C90E68F5B5AAB2E4FFC62FFB7202E447ED804DA7BE43ED0CF2D541B18DCFFFAA30999D3C501FF82016C6A64E233F0B4279CCAB08915A22E39092D5B3AC96345CF35417A90A3965D1AA371A88CDFD0702AE79AC8E559DE4E040FC53A102634C318259C63124A49C024D6B25D24B3E3F26B4C75753FABC17E63E244DCBBB2C3854F9441D81133489C3715E9B613711946DF04D8AFF9B42AB294856F6768C43BC1BA06F2E9A786D03240B9DC4074E2F6C53B75479593A25606196382CF6581D8B0AA4C4F1A857EA98BC884A1C98F75EC349E0C193846FC05F0CDA00F1BDAEFE2C10DB1FB3638222402D1BC477EEA446D8BECDA75BAEB115EEB565245194B231C2D2730A69FD62B186D900DD0A6CB30D705E2B17B560360974606B9DA216D4FABD0E282E4AD4828B27D1812E45885A40CB975C2E8C29320CD8CFF4E85CEC78946E6EC4EE4795099BE481B7ECE18B70D9B0591C987D3BB40F577DB061954B55471AD3A815E059C6B70A916586B67AA18D376FC26039A0D8DB01F1853EB7B2C518EAACCB84333208016BDAC25F29FEA805DC8ACA6A8262E915A5EE1A1D8F7AC5B1F0E71A6B6632B68C583793AF06C445DE71A72D3E728E484D38A90223E025EFA096E2A4FC2C57B90C01B60E661DF48AA3D50F07C99F3E78D3912F863CB1348658542B317D6E1EFF4FFB96960333EBDBCCD8A5864E1BCE221C85AD673226B565135A14012B2AA70A3D65B650B550FAD828676EA091BBD092EB69EDB8543C2A8E4ABDD2D86FC3C254897D36ECF960CC873C04B66335424C056B91E43F34479A584506B078AAF109AF831109CF5F3F5547C2C2137128ECB1DE214CDB298C3EFBFBB07E2CBE91ECB8FAF160EC89BC8266F72B545C5B33FFF8F0B3DB394E13A184DC4554A839356942F970A4E6A9EEA123FFCC51D30AABE03CC206AB8707E624732D874E5297BE7F52FFAEAFE5ECAFC4C8FF60037347A64CE23AA883BE7A9BFC7ECCF209712E18E50946CB5FFCF2B24493007962EF1E26691900EBBE3E3A7E4DFDB187E1FCE18571926C88F85CA18C3D3962CFA416EFE57D2D8DDAD58C45E506A817253197F017E1063E9EB9BF16394F9CC5E75B22F32BE72A462438718E9C7FE2D550D7806F2A107E05F1FA01C4BF0BC0E3EFCDC04841F96E80B4B8C906FD4CA9C85F152032465BA34E46A2EB06A4ED43DABC17DA96336E493F7653282FEA6D519FDC04EF05314FE342C99055BEB3D0FB25835EC1CB7B2FBF436B5149B83338A3FEDDC9AF7115BE4D68DAAEEFDDA97E1C0DEF4331B7ED1EC6B0A5AF874D5EF64E45097CE76D8D7C2DEF3A850DD6712E54D88565FF0E942DE4F252C5414D659852DED62C819278EE8CDB26F1DDE36871CFF19F5729DB4E3792AAD99D6CA84D199B37DFB2AA8C6D362807939EEE3C50B40C75A75162A5A68D26D53C21D77EEC4F76FE3C1CA5661B0E9395F4ED8C4A2938F73D4A439142EE3ED7A26591BB6D801CCA6214CE5487251DDCCBE606A916DC69E486B1BDD72249AA5A0F2A7B979A70B58A4D375558EDE23E6C427072FABC8AB2BD709F1691EDC47E5A28D674A019E15853205648D614691886CD88D4DE79B2F99899C6AB01EDED08A9F6E6E0438EDB60444C3B4AA3DAC2A5954F1557451C4322954F7576EEFB533325549CF8EA374A92633CC916632DD4AEFA65CFA649FAC215F49E89312D4A4B7C6C1DEDBD17C623D5CB99C3A4948136A87D2A51123018A6A6B2E80BA38EEC86E23029D349C7D370A85F942FD21FD6DE55375F98BE262B7E448F3CA9B8A72C9E595E1043D3C1BB08D1A39CB999896BCAB43579250935C5E8B224629A4CE992F4BCFAAC7AD4EB3490EB54AD6237894F55854FC5DAA856A13E4E6A2BBD4E2028D8542D54492C945FAE0549516545517E0D3A6A8F4AA54765A5EA7805FCDB22F20F783A89A710E8EF75D531E5B69C9224546C7BA965DADAE4F2B5A8A5031143E5787846AB8BFD2EF2D7D15876C137E6A5899C1A758BD04404B169DD3BEB0082A55D3B876BB59DB431F598D3BB0AA961030FCC200D41513642014D81B330DF602C7FCD60E26D1B88538419C23531F9ADD32CC2FBA89A8C5335AA92B0A738608366C69338F5EEC13A45AFD73049BCFC4F327D047E566C5BDEC1CD22BCCAD25D96A226C3E0CE2782C4F2B9BCA8FC423595ACF3E9D5AEF8EB6B369A80AAE96D8A1DDAB799E76FEA7ABFE36CA3B640E48B84FD0E773E9669BED3BD7DAA912EA3501168DF7DF5DA6605839D8FC092AB7009F2D363FDBAA1AFD939DC82F5531568D20E221F08B2DB4F671ED8C62048F6184D7EF4137178133CFEF05FBDA8BB747F950000 , N'6.1.3-40302')

