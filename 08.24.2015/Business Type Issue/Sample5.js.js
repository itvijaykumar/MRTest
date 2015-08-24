
var referenceWorkTypesJson = [{"Value":1,"Text":"Full Time – Paid"},{"Value":2,"Text":"Full Time - Unpaid"},{"Value":3,"Text":"Part Time – Paid"},{"Value":4,"Text":"Part Time – Unpaid"}];
var referenceReasonsForLeavingJson = [{"Value":2,"Text":"End of contract"},{"Value":3,"Text":"Resignation"},{"Value":4,"Text":"Redundancy"},{"Value":5,"Text":"Misconduct/Dismissal"},{"Value":6,"Text":"Not left/currently employed"},{"Value":21,"Text":"Other – please specify"}];
var referenceGapTypesJson = [{"Value":3,"Text":"Illness"},{"Value":1,"Text":"Jobseeker’s Allowance"},{"Value":4,"Text":"Looking for Work (not claiming benefits)"},{"Value":5,"Text":"Maternity"},{"Value":2,"Text":"Travelling"},{"Value":6,"Text":"Other"}];
var referenceQualificationTypesJson = [{"Value":1,"Text":"Master\u0027s Degree"},{"Value":2,"Text":"Undergraduate Degree"},{"Value":4,"Text":"No qualification obtained"},{"Value":3,"Text":"Other – please specify"}];    
var addressTypesJson = [{"Value":1,"Text":"Permanent home address"},{"Value":2,"Text":"Other address - where registered"},{"Value":3,"Text":"Temporary Address"}];
var countriesJson = [];
var nationalitiesJson = [{"Value":27,"Text":"British"},{"Value":1,"Text":"Afghan"},{"Value":2,"Text":"Albanian"},{"Value":3,"Text":"Algerian"},{"Value":4,"Text":"American"},{"Value":5,"Text":"Andorran"},{"Value":6,"Text":"Angolan"},{"Value":7,"Text":"Antiguans"},{"Value":8,"Text":"Argentinean"},{"Value":9,"Text":"Armenian"},{"Value":10,"Text":"Australian"},{"Value":11,"Text":"Austrian"},{"Value":12,"Text":"Azerbaijani"},{"Value":13,"Text":"Bahamian"},{"Value":14,"Text":"Bahraini"},{"Value":15,"Text":"Bangladeshi"},{"Value":16,"Text":"Barbadian"},{"Value":17,"Text":"Barbudans"},{"Value":18,"Text":"Batswana"},{"Value":19,"Text":"Belarusian"},{"Value":20,"Text":"Belgian"},{"Value":21,"Text":"Belizean"},{"Value":22,"Text":"Beninese"},{"Value":23,"Text":"Bhutanese"},{"Value":24,"Text":"Bolivian"},{"Value":25,"Text":"Bosnian"},{"Value":26,"Text":"Brazilian"},{"Value":28,"Text":"Bruneian"},{"Value":29,"Text":"Bulgarian"},{"Value":30,"Text":"Burkinabe"},{"Value":31,"Text":"Burmese"},{"Value":32,"Text":"Burundian"},{"Value":33,"Text":"Cambodian"},{"Value":34,"Text":"Cameroonian"},{"Value":35,"Text":"Canadian"},{"Value":36,"Text":"Cape Verdean"},{"Value":37,"Text":"Central African"},{"Value":38,"Text":"Chadian"},{"Value":39,"Text":"Chilean"},{"Value":40,"Text":"Chinese"},{"Value":41,"Text":"Colombian"},{"Value":42,"Text":"Comoran"},{"Value":43,"Text":"Congolese"},{"Value":44,"Text":"Costa Rican"},{"Value":45,"Text":"Croatian"},{"Value":46,"Text":"Cuban"},{"Value":47,"Text":"Cypriot"},{"Value":48,"Text":"Czech"},{"Value":49,"Text":"Danish"},{"Value":50,"Text":"Djibouti"},{"Value":51,"Text":"Dominican"},{"Value":52,"Text":"Dutch"},{"Value":53,"Text":"East Timorese"},{"Value":54,"Text":"Ecuadorean"},{"Value":55,"Text":"Egyptian"},{"Value":56,"Text":"Emirian"},{"Value":57,"Text":"Equatorial Guinean"},{"Value":58,"Text":"Eritrean"},{"Value":59,"Text":"Estonian"},{"Value":60,"Text":"Ethiopian"},{"Value":61,"Text":"Fijian"},{"Value":62,"Text":"Filipino"},{"Value":63,"Text":"Finnish"},{"Value":64,"Text":"French"},{"Value":65,"Text":"Gabonese"},{"Value":66,"Text":"Gambian"},{"Value":67,"Text":"Georgian"},{"Value":68,"Text":"German"},{"Value":69,"Text":"Ghanaian"},{"Value":70,"Text":"Greek"},{"Value":71,"Text":"Grenadian"},{"Value":72,"Text":"Guatemalan"},{"Value":73,"Text":"Guinea-Bissauan"},{"Value":74,"Text":"Guinean"},{"Value":75,"Text":"Guyanese"},{"Value":76,"Text":"Haitian"},{"Value":77,"Text":"Herzegovinian"},{"Value":78,"Text":"Honduran"},{"Value":79,"Text":"Hungarian"},{"Value":80,"Text":"Icelander"},{"Value":81,"Text":"Indian"},{"Value":82,"Text":"Indonesian"},{"Value":83,"Text":"Iranian"},{"Value":84,"Text":"Iraqi"},{"Value":85,"Text":"Irish"},{"Value":86,"Text":"Israeli"},{"Value":87,"Text":"Italian"},{"Value":88,"Text":"Ivorian"},{"Value":89,"Text":"Jamaican"},{"Value":90,"Text":"Japanese"},{"Value":91,"Text":"Jordanian"},{"Value":92,"Text":"Kazakhstani"},{"Value":93,"Text":"Kenyan"},{"Value":94,"Text":"Kittian and Nevisian"},{"Value":95,"Text":"Kuwaiti"},{"Value":96,"Text":"Kyrgyz"},{"Value":97,"Text":"Laotian"},{"Value":98,"Text":"Latvian"},{"Value":99,"Text":"Lebanese"},{"Value":100,"Text":"Liberian"},{"Value":101,"Text":"Libyan"},{"Value":102,"Text":"Liechtensteiner"},{"Value":103,"Text":"Lithuanian"},{"Value":104,"Text":"Luxembourger"},{"Value":105,"Text":"Macedonian"},{"Value":106,"Text":"Malagasy"},{"Value":107,"Text":"Malawian"},{"Value":108,"Text":"Malaysian"},{"Value":109,"Text":"Maldivan"},{"Value":110,"Text":"Malian"},{"Value":111,"Text":"Maltese"},{"Value":112,"Text":"Marshallese"},{"Value":113,"Text":"Mauritanian"},{"Value":114,"Text":"Mauritian"},{"Value":115,"Text":"Mexican"},{"Value":116,"Text":"Micronesian"},{"Value":117,"Text":"Moldovan"},{"Value":118,"Text":"Monacan"},{"Value":119,"Text":"Mongolian"},{"Value":120,"Text":"Moroccan"},{"Value":121,"Text":"Mosotho"},{"Value":122,"Text":"Motswana"},{"Value":123,"Text":"Mozambican"},{"Value":124,"Text":"Namibian"},{"Value":125,"Text":"Nauruan"},{"Value":126,"Text":"Nepalese"},{"Value":127,"Text":"Netherlander"},{"Value":128,"Text":"New Zealander"},{"Value":129,"Text":"Ni-Vanuatu"},{"Value":130,"Text":"Nicaraguan"},{"Value":131,"Text":"Nigerian"},{"Value":132,"Text":"Nigerien"},{"Value":133,"Text":"North Korean"},{"Value":134,"Text":"Northern Irish"},{"Value":135,"Text":"Norwegian"},{"Value":136,"Text":"Omani"},{"Value":137,"Text":"Pakistani"},{"Value":138,"Text":"Palauan"},{"Value":139,"Text":"Panamanian"},{"Value":140,"Text":"Papua New Guinean"},{"Value":141,"Text":"Paraguayan"},{"Value":142,"Text":"Peruvian"},{"Value":143,"Text":"Polish"},{"Value":144,"Text":"Portuguese"},{"Value":145,"Text":"Qatari"},{"Value":146,"Text":"Romanian"},{"Value":147,"Text":"Russian"},{"Value":148,"Text":"Rwandan"},{"Value":149,"Text":"Saint Lucian"},{"Value":150,"Text":"Salvadoran"},{"Value":151,"Text":"Samoan"},{"Value":152,"Text":"San Marinese"},{"Value":153,"Text":"Sao Tomean"},{"Value":154,"Text":"Saudi"},{"Value":155,"Text":"Scottish"},{"Value":156,"Text":"Senegalese"},{"Value":157,"Text":"Serbian"},{"Value":158,"Text":"Seychellois"},{"Value":159,"Text":"Sierra Leonean"},{"Value":160,"Text":"Singaporean"},{"Value":161,"Text":"Slovakian"},{"Value":162,"Text":"Slovenian"},{"Value":163,"Text":"Solomon Islander"},{"Value":164,"Text":"Somali"},{"Value":165,"Text":"South African"},{"Value":166,"Text":"South Korean"},{"Value":167,"Text":"Spanish"},{"Value":168,"Text":"Sri Lankan"},{"Value":169,"Text":"Sudanese"},{"Value":170,"Text":"Surinamer"},{"Value":171,"Text":"Swazi"},{"Value":172,"Text":"Swedish"},{"Value":173,"Text":"Swiss"},{"Value":174,"Text":"Syrian"},{"Value":175,"Text":"Taiwanese"},{"Value":176,"Text":"Tajik"},{"Value":177,"Text":"Tanzanian"},{"Value":178,"Text":"Thai"},{"Value":179,"Text":"Togolese"},{"Value":180,"Text":"Tongan"},{"Value":181,"Text":"Trinidadian or Tobagonian"},{"Value":182,"Text":"Tunisian"},{"Value":183,"Text":"Turkish"},{"Value":184,"Text":"Tuvaluan"},{"Value":185,"Text":"Ugandan"},{"Value":186,"Text":"Ukrainian"},{"Value":187,"Text":"Uruguayan"},{"Value":188,"Text":"Uzbekistani"},{"Value":189,"Text":"Venezuelan"},{"Value":190,"Text":"Vietnamese"},{"Value":191,"Text":"Welsh"},{"Value":192,"Text":"Yemenite"},{"Value":193,"Text":"Zambian"},{"Value":194,"Text":"Zimbabwean"}];
var countiesJson = [];
var referenceReasonsForLeavingFull = [{"Value":1,"Text":"Not company policy to provide this info"},{"Value":2,"Text":"End of contract"},{"Value":3,"Text":"Resignation"},{"Value":4,"Text":"Redundancy"},{"Value":5,"Text":"Misconduct/Dismissal"},{"Value":6,"Text":"Not left/currently employed"},{"Value":21,"Text":"Other – please specify"}];
var temporaryAddressTypesJson = [{"Value":2,"Text":"B\u0026B/Hotel"},{"Value":3,"Text":"Staying with friends/family"},{"Value":4,"Text":" University Accommodation"},{"Value":5,"Text":"Short-term let"},{"Value":1,"Text":"Other"}];
var rolesForExperienceRegistrationTypeAndSector = {"1":{"1":[{"ChildRoles":[{"ParentMarketSectorRoleId":1,"DisplayOrder":1,"MarketSectorRoleId":2,"RoleName":"Assessor"},{"ParentMarketSectorRoleId":1,"DisplayOrder":2,"MarketSectorRoleId":3,"RoleName":"Coaching"},{"ParentMarketSectorRoleId":1,"DisplayOrder":3,"MarketSectorRoleId":4,"RoleName":"Deliverer"},{"ParentMarketSectorRoleId":1,"DisplayOrder":4,"MarketSectorRoleId":5,"RoleName":"Designer"},{"ParentMarketSectorRoleId":1,"DisplayOrder":5,"MarketSectorRoleId":6,"RoleName":"Lead Designer"},{"ParentMarketSectorRoleId":1,"DisplayOrder":6,"MarketSectorRoleId":7,"RoleName":"TNA"}],"DisplayOrder":1,"MarketSectorRoleId":1,"RoleName":"Training "},{"ChildRoles":[{"ParentMarketSectorRoleId":8,"DisplayOrder":7,"MarketSectorRoleId":9,"RoleName":"Business Analyst"},{"ParentMarketSectorRoleId":8,"DisplayOrder":8,"MarketSectorRoleId":10,"RoleName":"Case Handler"},{"ParentMarketSectorRoleId":8,"DisplayOrder":9,"MarketSectorRoleId":11,"RoleName":"Complaint Handler"},{"ParentMarketSectorRoleId":8,"DisplayOrder":10,"MarketSectorRoleId":12,"RoleName":"Data Gatherer/Adminstrator"},{"ParentMarketSectorRoleId":8,"DisplayOrder":11,"MarketSectorRoleId":13,"RoleName":"Operations Manager"},{"ParentMarketSectorRoleId":8,"DisplayOrder":12,"MarketSectorRoleId":14,"RoleName":"Paraplanning"},{"ParentMarketSectorRoleId":8,"DisplayOrder":13,"MarketSectorRoleId":15,"RoleName":"Programme Manager"},{"ParentMarketSectorRoleId":8,"DisplayOrder":14,"MarketSectorRoleId":16,"RoleName":"Project Manager"},{"ParentMarketSectorRoleId":8,"DisplayOrder":15,"MarketSectorRoleId":17,"RoleName":"Quality Assurer"},{"ParentMarketSectorRoleId":8,"DisplayOrder":16,"MarketSectorRoleId":18,"RoleName":"Team Leader"}],"DisplayOrder":2,"MarketSectorRoleId":8,"RoleName":"Business Review"},{"ChildRoles":[{"ParentMarketSectorRoleId":19,"DisplayOrder":17,"MarketSectorRoleId":20,"RoleName":"IT Developer"},{"ParentMarketSectorRoleId":19,"DisplayOrder":18,"MarketSectorRoleId":21,"RoleName":"MI Analyst"}],"DisplayOrder":3,"MarketSectorRoleId":19,"RoleName":"Other"}],"2":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":134,"RoleName":"Any role"}],"3":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":135,"RoleName":"Any role"}],"4":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":136,"RoleName":"Any role"}],"5":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":137,"RoleName":"Any role"}],"6":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":138,"RoleName":"Any role"}],"7":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":139,"RoleName":"Any role"}],"8":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":140,"RoleName":"Any role"}]},"2":{"1":[{"ChildRoles":[{"ParentMarketSectorRoleId":141,"DisplayOrder":1,"MarketSectorRoleId":142,"RoleName":"Assessor"},{"ParentMarketSectorRoleId":141,"DisplayOrder":2,"MarketSectorRoleId":143,"RoleName":"Coaching"},{"ParentMarketSectorRoleId":141,"DisplayOrder":3,"MarketSectorRoleId":144,"RoleName":"Deliverer"},{"ParentMarketSectorRoleId":141,"DisplayOrder":4,"MarketSectorRoleId":145,"RoleName":"Designer"},{"ParentMarketSectorRoleId":141,"DisplayOrder":5,"MarketSectorRoleId":146,"RoleName":"Lead Designer"},{"ParentMarketSectorRoleId":141,"DisplayOrder":6,"MarketSectorRoleId":147,"RoleName":"TNA"}],"DisplayOrder":1,"MarketSectorRoleId":141,"RoleName":"Training "},{"ChildRoles":[{"ParentMarketSectorRoleId":148,"DisplayOrder":7,"MarketSectorRoleId":149,"RoleName":"Business Analyst"},{"ParentMarketSectorRoleId":148,"DisplayOrder":8,"MarketSectorRoleId":150,"RoleName":"Case Handler"},{"ParentMarketSectorRoleId":148,"DisplayOrder":9,"MarketSectorRoleId":151,"RoleName":"Complaint Handler"},{"ParentMarketSectorRoleId":148,"DisplayOrder":10,"MarketSectorRoleId":152,"RoleName":"Data Gatherer/Adminstrator"},{"ParentMarketSectorRoleId":148,"DisplayOrder":11,"MarketSectorRoleId":153,"RoleName":"Operations Manager"},{"ParentMarketSectorRoleId":148,"DisplayOrder":12,"MarketSectorRoleId":154,"RoleName":"Paraplanning"},{"ParentMarketSectorRoleId":148,"DisplayOrder":13,"MarketSectorRoleId":155,"RoleName":"Programme Manager"},{"ParentMarketSectorRoleId":148,"DisplayOrder":14,"MarketSectorRoleId":156,"RoleName":"Project Manager"},{"ParentMarketSectorRoleId":148,"DisplayOrder":15,"MarketSectorRoleId":157,"RoleName":"Quality Assurer"},{"ParentMarketSectorRoleId":148,"DisplayOrder":16,"MarketSectorRoleId":158,"RoleName":"Team Leader"}],"DisplayOrder":2,"MarketSectorRoleId":148,"RoleName":"Business Review"},{"ChildRoles":[{"ParentMarketSectorRoleId":159,"DisplayOrder":17,"MarketSectorRoleId":160,"RoleName":"IT Developer"},{"ParentMarketSectorRoleId":159,"DisplayOrder":18,"MarketSectorRoleId":161,"RoleName":"MI Analyst"}],"DisplayOrder":3,"MarketSectorRoleId":159,"RoleName":"Other"}],"2":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":162,"RoleName":"Any role"}],"3":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":163,"RoleName":"Any role"}],"4":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":164,"RoleName":"Any role"}],"5":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":165,"RoleName":"Any role"}],"6":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":166,"RoleName":"Any role"}],"7":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":167,"RoleName":"Any role"}],"8":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":168,"RoleName":"Any role"}]},"3":{"1":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":22,"RoleName":"Board Level"},{"ChildRoles":[{"ParentMarketSectorRoleId":23,"DisplayOrder":1,"MarketSectorRoleId":24,"RoleName":"Finance"},{"ParentMarketSectorRoleId":23,"DisplayOrder":2,"MarketSectorRoleId":25,"RoleName":"Operations"},{"ParentMarketSectorRoleId":23,"DisplayOrder":3,"MarketSectorRoleId":26,"RoleName":"Technical"},{"ParentMarketSectorRoleId":23,"DisplayOrder":4,"MarketSectorRoleId":27,"RoleName":"HR/Legal"},{"ParentMarketSectorRoleId":23,"DisplayOrder":5,"MarketSectorRoleId":28,"RoleName":"Sales \u0026 Marketing"}],"DisplayOrder":2,"MarketSectorRoleId":23,"RoleName":"Director"},{"ChildRoles":[{"ParentMarketSectorRoleId":29,"DisplayOrder":6,"MarketSectorRoleId":30,"RoleName":"Finance"},{"ParentMarketSectorRoleId":29,"DisplayOrder":7,"MarketSectorRoleId":31,"RoleName":"Operations"},{"ParentMarketSectorRoleId":29,"DisplayOrder":8,"MarketSectorRoleId":32,"RoleName":"Technical"},{"ParentMarketSectorRoleId":29,"DisplayOrder":9,"MarketSectorRoleId":33,"RoleName":"HR/Legal"},{"ParentMarketSectorRoleId":29,"DisplayOrder":10,"MarketSectorRoleId":34,"RoleName":"Sales \u0026 Marketing"}],"DisplayOrder":3,"MarketSectorRoleId":29,"RoleName":"Manager"},{"ChildRoles":[],"DisplayOrder":4,"MarketSectorRoleId":35,"RoleName":"Consultant"}],"2":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":36,"RoleName":"Board Level"},{"ChildRoles":[{"ParentMarketSectorRoleId":37,"DisplayOrder":1,"MarketSectorRoleId":38,"RoleName":"Finance"},{"ParentMarketSectorRoleId":37,"DisplayOrder":2,"MarketSectorRoleId":39,"RoleName":"Operations"},{"ParentMarketSectorRoleId":37,"DisplayOrder":3,"MarketSectorRoleId":40,"RoleName":"Technical"},{"ParentMarketSectorRoleId":37,"DisplayOrder":4,"MarketSectorRoleId":41,"RoleName":"HR/Legal"},{"ParentMarketSectorRoleId":37,"DisplayOrder":5,"MarketSectorRoleId":42,"RoleName":"Sales \u0026 Marketing"}],"DisplayOrder":2,"MarketSectorRoleId":37,"RoleName":"Director"},{"ChildRoles":[{"ParentMarketSectorRoleId":43,"DisplayOrder":6,"MarketSectorRoleId":44,"RoleName":"Finance"},{"ParentMarketSectorRoleId":43,"DisplayOrder":7,"MarketSectorRoleId":45,"RoleName":"Operations"},{"ParentMarketSectorRoleId":43,"DisplayOrder":8,"MarketSectorRoleId":46,"RoleName":"Technical"},{"ParentMarketSectorRoleId":43,"DisplayOrder":9,"MarketSectorRoleId":47,"RoleName":"HR/Legal"},{"ParentMarketSectorRoleId":43,"DisplayOrder":10,"MarketSectorRoleId":48,"RoleName":"Sales \u0026 Marketing"}],"DisplayOrder":3,"MarketSectorRoleId":43,"RoleName":"Manager"},{"ChildRoles":[],"DisplayOrder":4,"MarketSectorRoleId":49,"RoleName":"Consultant"}],"3":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":50,"RoleName":"Board Level"},{"ChildRoles":[{"ParentMarketSectorRoleId":51,"DisplayOrder":1,"MarketSectorRoleId":52,"RoleName":"Finance"},{"ParentMarketSectorRoleId":51,"DisplayOrder":2,"MarketSectorRoleId":53,"RoleName":"Operations"},{"ParentMarketSectorRoleId":51,"DisplayOrder":3,"MarketSectorRoleId":54,"RoleName":"Technical"},{"ParentMarketSectorRoleId":51,"DisplayOrder":4,"MarketSectorRoleId":55,"RoleName":"HR/Legal"},{"ParentMarketSectorRoleId":51,"DisplayOrder":5,"MarketSectorRoleId":56,"RoleName":"Sales \u0026 Marketing"}],"DisplayOrder":2,"MarketSectorRoleId":51,"RoleName":"Director"},{"ChildRoles":[{"ParentMarketSectorRoleId":57,"DisplayOrder":6,"MarketSectorRoleId":58,"RoleName":"Finance"},{"ParentMarketSectorRoleId":57,"DisplayOrder":7,"MarketSectorRoleId":59,"RoleName":"Operations"},{"ParentMarketSectorRoleId":57,"DisplayOrder":8,"MarketSectorRoleId":60,"RoleName":"Technical"},{"ParentMarketSectorRoleId":57,"DisplayOrder":9,"MarketSectorRoleId":61,"RoleName":"HR/Legal"},{"ParentMarketSectorRoleId":57,"DisplayOrder":10,"MarketSectorRoleId":62,"RoleName":"Sales \u0026 Marketing"}],"DisplayOrder":3,"MarketSectorRoleId":57,"RoleName":"Manager"},{"ChildRoles":[],"DisplayOrder":4,"MarketSectorRoleId":63,"RoleName":"Consultant"}],"4":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":64,"RoleName":"Board Level"},{"ChildRoles":[{"ParentMarketSectorRoleId":65,"DisplayOrder":1,"MarketSectorRoleId":66,"RoleName":"Finance"},{"ParentMarketSectorRoleId":65,"DisplayOrder":2,"MarketSectorRoleId":67,"RoleName":"Operations"},{"ParentMarketSectorRoleId":65,"DisplayOrder":3,"MarketSectorRoleId":68,"RoleName":"Technical"},{"ParentMarketSectorRoleId":65,"DisplayOrder":4,"MarketSectorRoleId":69,"RoleName":"HR/Legal"},{"ParentMarketSectorRoleId":65,"DisplayOrder":5,"MarketSectorRoleId":70,"RoleName":"Sales \u0026 Marketing"}],"DisplayOrder":2,"MarketSectorRoleId":65,"RoleName":"Director"},{"ChildRoles":[{"ParentMarketSectorRoleId":71,"DisplayOrder":6,"MarketSectorRoleId":72,"RoleName":"Finance"},{"ParentMarketSectorRoleId":71,"DisplayOrder":7,"MarketSectorRoleId":73,"RoleName":"Operations"},{"ParentMarketSectorRoleId":71,"DisplayOrder":8,"MarketSectorRoleId":74,"RoleName":"Technical"},{"ParentMarketSectorRoleId":71,"DisplayOrder":9,"MarketSectorRoleId":75,"RoleName":"HR/Legal"},{"ParentMarketSectorRoleId":71,"DisplayOrder":10,"MarketSectorRoleId":76,"RoleName":"Sales \u0026 Marketing"}],"DisplayOrder":3,"MarketSectorRoleId":71,"RoleName":"Manager"},{"ChildRoles":[],"DisplayOrder":4,"MarketSectorRoleId":77,"RoleName":"Consultant"}],"5":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":78,"RoleName":"Board Level"},{"ChildRoles":[{"ParentMarketSectorRoleId":79,"DisplayOrder":1,"MarketSectorRoleId":80,"RoleName":"Finance"},{"ParentMarketSectorRoleId":79,"DisplayOrder":2,"MarketSectorRoleId":81,"RoleName":"Operations"},{"ParentMarketSectorRoleId":79,"DisplayOrder":3,"MarketSectorRoleId":82,"RoleName":"Technical"},{"ParentMarketSectorRoleId":79,"DisplayOrder":4,"MarketSectorRoleId":83,"RoleName":"HR/Legal"},{"ParentMarketSectorRoleId":79,"DisplayOrder":5,"MarketSectorRoleId":84,"RoleName":"Sales \u0026 Marketing"}],"DisplayOrder":2,"MarketSectorRoleId":79,"RoleName":"Director"},{"ChildRoles":[{"ParentMarketSectorRoleId":85,"DisplayOrder":6,"MarketSectorRoleId":86,"RoleName":"Finance"},{"ParentMarketSectorRoleId":85,"DisplayOrder":7,"MarketSectorRoleId":87,"RoleName":"Operations"},{"ParentMarketSectorRoleId":85,"DisplayOrder":8,"MarketSectorRoleId":88,"RoleName":"Technical"},{"ParentMarketSectorRoleId":85,"DisplayOrder":9,"MarketSectorRoleId":89,"RoleName":"HR/Legal"},{"ParentMarketSectorRoleId":85,"DisplayOrder":10,"MarketSectorRoleId":90,"RoleName":"Sales \u0026 Marketing"}],"DisplayOrder":3,"MarketSectorRoleId":85,"RoleName":"Manager"},{"ChildRoles":[],"DisplayOrder":4,"MarketSectorRoleId":91,"RoleName":"Consultant"}],"6":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":92,"RoleName":"Board Level"},{"ChildRoles":[{"ParentMarketSectorRoleId":93,"DisplayOrder":1,"MarketSectorRoleId":94,"RoleName":"Finance"},{"ParentMarketSectorRoleId":93,"DisplayOrder":2,"MarketSectorRoleId":95,"RoleName":"Operations"},{"ParentMarketSectorRoleId":93,"DisplayOrder":3,"MarketSectorRoleId":96,"RoleName":"Technical"},{"ParentMarketSectorRoleId":93,"DisplayOrder":4,"MarketSectorRoleId":97,"RoleName":"HR/Legal"},{"ParentMarketSectorRoleId":93,"DisplayOrder":5,"MarketSectorRoleId":98,"RoleName":"Sales \u0026 Marketing"}],"DisplayOrder":2,"MarketSectorRoleId":93,"RoleName":"Director"},{"ChildRoles":[{"ParentMarketSectorRoleId":99,"DisplayOrder":6,"MarketSectorRoleId":100,"RoleName":"Finance"},{"ParentMarketSectorRoleId":99,"DisplayOrder":7,"MarketSectorRoleId":101,"RoleName":"Operations"},{"ParentMarketSectorRoleId":99,"DisplayOrder":8,"MarketSectorRoleId":102,"RoleName":"Technical"},{"ParentMarketSectorRoleId":99,"DisplayOrder":9,"MarketSectorRoleId":103,"RoleName":"HR/Legal"},{"ParentMarketSectorRoleId":99,"DisplayOrder":10,"MarketSectorRoleId":104,"RoleName":"Sales \u0026 Marketing"}],"DisplayOrder":3,"MarketSectorRoleId":99,"RoleName":"Manager"},{"ChildRoles":[],"DisplayOrder":4,"MarketSectorRoleId":105,"RoleName":"Consultant"}],"7":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":106,"RoleName":"Board Level"},{"ChildRoles":[{"ParentMarketSectorRoleId":107,"DisplayOrder":1,"MarketSectorRoleId":108,"RoleName":"Finance"},{"ParentMarketSectorRoleId":107,"DisplayOrder":2,"MarketSectorRoleId":109,"RoleName":"Operations"},{"ParentMarketSectorRoleId":107,"DisplayOrder":3,"MarketSectorRoleId":110,"RoleName":"Technical"},{"ParentMarketSectorRoleId":107,"DisplayOrder":4,"MarketSectorRoleId":111,"RoleName":"HR/Legal"},{"ParentMarketSectorRoleId":107,"DisplayOrder":5,"MarketSectorRoleId":112,"RoleName":"Sales \u0026 Marketing"}],"DisplayOrder":2,"MarketSectorRoleId":107,"RoleName":"Director"},{"ChildRoles":[{"ParentMarketSectorRoleId":113,"DisplayOrder":6,"MarketSectorRoleId":114,"RoleName":"Finance"},{"ParentMarketSectorRoleId":113,"DisplayOrder":7,"MarketSectorRoleId":115,"RoleName":"Operations"},{"ParentMarketSectorRoleId":113,"DisplayOrder":8,"MarketSectorRoleId":116,"RoleName":"Technical"},{"ParentMarketSectorRoleId":113,"DisplayOrder":9,"MarketSectorRoleId":117,"RoleName":"HR/Legal"},{"ParentMarketSectorRoleId":113,"DisplayOrder":10,"MarketSectorRoleId":118,"RoleName":"Sales \u0026 Marketing"}],"DisplayOrder":3,"MarketSectorRoleId":113,"RoleName":"Manager"},{"ChildRoles":[],"DisplayOrder":4,"MarketSectorRoleId":119,"RoleName":"Consultant"}],"8":[{"ChildRoles":[],"DisplayOrder":1,"MarketSectorRoleId":120,"RoleName":"Board Level"},{"ChildRoles":[{"ParentMarketSectorRoleId":121,"DisplayOrder":1,"MarketSectorRoleId":122,"RoleName":"Finance"},{"ParentMarketSectorRoleId":121,"DisplayOrder":2,"MarketSectorRoleId":123,"RoleName":"Operations"},{"ParentMarketSectorRoleId":121,"DisplayOrder":3,"MarketSectorRoleId":124,"RoleName":"Technical"},{"ParentMarketSectorRoleId":121,"DisplayOrder":4,"MarketSectorRoleId":125,"RoleName":"HR/Legal"},{"ParentMarketSectorRoleId":121,"DisplayOrder":5,"MarketSectorRoleId":126,"RoleName":"Sales \u0026 Marketing"}],"DisplayOrder":2,"MarketSectorRoleId":121,"RoleName":"Director"},{"ChildRoles":[{"ParentMarketSectorRoleId":127,"DisplayOrder":6,"MarketSectorRoleId":128,"RoleName":"Finance"},{"ParentMarketSectorRoleId":127,"DisplayOrder":7,"MarketSectorRoleId":129,"RoleName":"Operations"},{"ParentMarketSectorRoleId":127,"DisplayOrder":8,"MarketSectorRoleId":130,"RoleName":"Technical"},{"ParentMarketSectorRoleId":127,"DisplayOrder":9,"MarketSectorRoleId":131,"RoleName":"HR/Legal"},{"ParentMarketSectorRoleId":127,"DisplayOrder":10,"MarketSectorRoleId":132,"RoleName":"Sales \u0026 Marketing"}],"DisplayOrder":3,"MarketSectorRoleId":127,"RoleName":"Manager"},{"ChildRoles":[],"DisplayOrder":4,"MarketSectorRoleId":133,"RoleName":"Consultant"}]}};