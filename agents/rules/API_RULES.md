# Aturan Kontrak API Backend Quilvian

Aturan ini menjaga konvensi backend yang sudah ada. `AGENTS.md` tetap menjadi pemegang wewenang; pakai implementasi matang terdekat di dalam domain pemiliknya, jangan memperkenalkan konvensi tandingan yang berjalan sejajar.

## Keselarasan dengan QBE canonical

Baca `agents/rules/engineering/BACKEND_ENGINEERING_CONTRACT.md` sebelum mengerjakan API. Terapkan QBE-SVC-001, QBE-API-001, QBE-PERM-001, QBE-LOG-001, QBE-DTO-001, QBE-VAL-001, dan aturan QBE-CODE yang berlaku. Implementasi rujukan hanya menggambarkan perilaku yang sudah ada; ia tidak menimpa kontrak canonical.

## Wewenang dan cakupan kontrak

- Source backend adalah pemegang wewenang atas kontrak API serta perilaku bisnis/keamanan. Kode frontend adalah rujukan konsumen, bukan wewenang yang boleh diam-diam mendefinisikan ulang kontrak.
- Sebelum mengerjakan API, periksa route/action controller yang sebenarnya, HTTP verb, DTO request dan response, validation, authorization, nilai status, perilaku pagination/filter, dan aturan workflow. Jangan menebak kontrak dari URL frontend.
- Jaga kompatibilitas mundur sejauh dapat dilakukan. Jangan mengganti nama, menghapus, atau merusak endpoint, field, pembungkus (envelope), nilai enum/status, atau action tanpa wewenang eksplisit dan penilaian dampak terhadap konsumen.
- Jangan memperkenalkan pembungkus response, arsitektur DTO, arsitektur validation, model error, atau abstraksi repository yang baru kecuali diberi wewenang eksplisit.

## Konvensi controller, route, dan response

- Ikuti konvensi controller terdekat: `[ApiController]`, `ControllerBase`, route bertversi `api/v1/...`, penamaan Area/domain, metadata Swagger, HTTP verb, binding, dan kode status.
- Pakai pembungkus sukses/gagal `ApiResponse<T>` dan bentuk pagination `PagedResult<T>` yang sudah mapan bila keluarga endpoint yang ada memang memakainya. Pertahankan filter, sorting, nilai bawaan, dan nama field response.
- Simpan DTO request dan response di folder `DTOs/` milik domain pemiliknya bila pola itu memang ada. Jangan mengekspos entity EF hanya untuk menghindari pemakaian response DTO yang sudah mapan.
- Pertahankan perilaku nullable, identifier, tanggal/waktu, nilai bawaan, dan data annotation. Pakai atribut validation dari DTO terdekat seperti `[Required]`, `[MaxLength]`, dan `[Range]` bila berlaku.
- Pakai API async dan teruskan `CancellationToken` mengikuti pola controller/service terdekat. Kembalikan semantik error/status yang sudah ada; jangan menutupi kegagalan dengan payload sukses yang dikarang.

## Authorization, ownership, dan wewenang workflow

- Pertahankan `[Authorize]`, `[AccessController]`, `[AccessAction]`, `[AccessPermission]`, `AccessTypes`, pemeriksaan role/permission, dan resolusi current-user/claim sebagaimana diimplementasikan domain pemiliknya.
- Untuk endpoint self-service, turunkan ownership dari current user yang terautentikasi memakai pola context/service yang sudah ada. Jangan menerima identifier actor, workforce, atau user sembarangan yang memungkinkan ownership dilangkahi.
- Backend tetap menjadi pemegang wewenang atas transisi workflow, authorization actor/delegated-actor, `AvailableActions`, approval/rejection, transisi status, dan idempotency. Apa yang terlihat di frontend bukan authorization.

## Bukti representatif

- Controller, route, DTO, dan model master data: `Areas/Administrator/MasterData/Controllers/BankController.cs`; `Areas/Administrator/MasterData/DTOs/BankDtos.cs`; `Areas/Administrator/MasterData/Models/MstBank.cs`
- Kontrak response bersama: `Responses/ApiResponse.cs`; `Responses/PagedResult.cs`
- Metadata authorization: `Attributes/AccessControllerAttribute.cs`; `Attributes/AccessActionAttribute.cs`; `Attributes/AccessPermissionAttribute.cs`
- Pola self-service/current-user: `Areas/SelfServices/HumanResource/Controllers/OvertimeSelfServiceController.cs`; `Areas/SelfServices/HumanResource/Services/OvertimeSelfServiceContextService.cs`
- Wewenang workflow: `Areas/Corporate/HumanResource/WorkflowManagement/Controllers/WorkflowActionV2Controller.cs`; `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowService.cs`; `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowService.ActionsV2.cs`

## Konsistensi lintas developer

Modul baru mengikuti implementasi matang terdekat di domainnya. Status "modul baru" bukan alasan yang membenarkan tata bahasa route, bentuk response, tata letak DTO, gaya validation, arsitektur persistence, atau model error yang baru.
