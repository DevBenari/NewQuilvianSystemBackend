# Medical Fee — Matriks Izin dan Audit

| Field | Nilai |
|---|---|
| Kontrak | `MDF-PERM-1.0` — `locked` 20 September 2026 |
| Blueprint ID | `MF-BP-001` |
| Mekanisme | `[AccessController]`, `[AccessAction]`, `[AccessPermission("<Resource>", "<Action>")]` |

---

## 1. Resource yang didaftarkan

| Resource | Cakupan |
|---|---|
| `MedicalFeeRole` | Daftar peran |
| `MedicalFeeSharingAgreement` | Kesepakatan tarif dan baris tarifnya |
| `MedicalFeePeriod` | Periode dan siklusnya |
| `MedicalFeeServiceFee` | Hasil jasa, rincian, koreksi |
| `MedicalFeeUnresolvedService` | Layanan yang belum dapat dihitung |
| `MedicalFeeHandoff` | Penyerahan ke Finance |

## 2. Matriks izin per endpoint

| Endpoint | Resource | Action |
|---|---|---|
| `GET /master-data/fee-roles*` | `MedicalFeeRole` | `View` |
| `POST`, `PUT`, `PATCH /master-data/fee-roles*` | `MedicalFeeRole` | `Manage` |
| `DELETE /master-data/fee-roles/{id}` | `MedicalFeeRole` | `Manage` |
| `GET /master-data/sharing-agreements*` | `MedicalFeeSharingAgreement` | `View` |
| `POST`, `PUT /master-data/sharing-agreements` | `MedicalFeeSharingAgreement` | `Manage` |
| `POST /.../activation`, `/termination` | `MedicalFeeSharingAgreement` | `Approve` |
| `POST /.../rules`, `/rules/{id}/supersede` | `MedicalFeeSharingAgreement` | `Manage` |
| `GET /fee-periods*` | `MedicalFeePeriod` | `View` |
| `POST /fee-periods` | `MedicalFeePeriod` | `Manage` |
| `POST /fee-periods/{id}/calculation`, `/reopen` | `MedicalFeePeriod` | `Calculate` |
| `POST /fee-periods/{id}/verification`, `/return` | `MedicalFeePeriod` | `Verify` |
| `POST /fee-periods/{id}/approval` | `MedicalFeePeriod` | `Approve` |
| `POST /fee-periods/{id}/closure` | `MedicalFeePeriod` | `Close` |
| `GET /service-fees*` | `MedicalFeeServiceFee` | `View` |
| `GET /service-fees/{id}/details` | `MedicalFeeServiceFee` | `View` |
| `POST /service-fees/{id}/verification`, `/return` | `MedicalFeeServiceFee` | `Verify` |
| `POST /service-fees/{id}/adjustments` | `MedicalFeeServiceFee` | `RequestAdjustment` |
| `POST /.../adjustments/{id}/approval`, `/rejection` | `MedicalFeeServiceFee` | `ApproveAdjustment` |
| `GET /unresolved-services*` | `MedicalFeeUnresolvedService` | `View` |
| `POST /unresolved-services/{id}/waiver` | `MedicalFeeUnresolvedService` | `Waive` |
| `GET /finance-handoffs*` | `MedicalFeeHandoff` | `View` |
| `POST /finance-handoffs/{id}/acknowledgement`, `/failure` | `MedicalFeeHandoff` | `Acknowledge` |
| `POST /finance-handoffs/{id}/resend` | `MedicalFeeHandoff` | `Manage` |

## 3. Pemisahan wewenang

`MDF-DES-017`, `MF-DEC-009`. Tiga pasangan izin yang MUST NOT dipegang satu peran jabatan:

| Pasangan | Mengapa |
|---|---|
| `MedicalFeePeriod.Verify` dan `MedicalFeePeriod.Approve` | Satu orang tidak boleh memverifikasi lalu menyetujui seluruh jasa satu bulan |
| `MedicalFeeServiceFee.RequestAdjustment` dan `.ApproveAdjustment` | Satu orang tidak boleh mengajukan lalu menyetujui koreksi nilai uang |
| `MedicalFeeSharingAgreement.Manage` dan `.Approve` | Satu orang tidak boleh menyusun tarif lalu mengaktifkannya sendiri |

Pemisahan ini ditegakkan **tiga lapis**, mengikuti pola `FIN-DES-014`:

| Lapis | Yang dicegah |
|---|---|
| Pemisahan izin | Peran jabatan tidak diberi kedua izin sekaligus |
| Pemeriksaan service | Pengguna yang sama dicegah walau izinnya lengkap |
| Check constraint | Barisnya tidak dapat tersimpan walau ada jalur kode lain kelak |

Lapis ketiga yang paling sering dianggap berlebihan, dan justru yang paling berguna: ia tetap
berlaku ketika kelak ada service baru, skrip perbaikan data, atau endpoint yang belum
terbayang sekarang.

### 3.1 Izin `Waive` berdiri sendiri

`MedicalFeeUnresolvedService.Waive` **tidak** digabungkan ke `Verify` mana pun. Mengesampingkan
layanan yang belum terhitung berarti memutuskan bahwa jasa seseorang memang tidak akan terbit —
keputusan yang berbeda sifatnya dari memverifikasi angka, dan pantas dipegang lebih sedikit
orang.

## 4. Peran jabatan yang diusulkan

| Peran jabatan | Izin |
|---|---|
| Staf Medical Fee | `MedicalFeeRole.View`, `MedicalFeeSharingAgreement.View` + `Manage`, `MedicalFeePeriod.View` + `Manage` + `Calculate`, `MedicalFeeServiceFee.View` + `RequestAdjustment`, `MedicalFeeUnresolvedService.View`, `MedicalFeeHandoff.View` |
| Supervisor Medical Fee | Seluruh `View`, `MedicalFeePeriod.Verify`, `MedicalFeeServiceFee.Verify`, `MedicalFeeUnresolvedService.View` |
| Manajer Medical Fee | Seluruh `View`, `MedicalFeePeriod.Approve` + `Close`, `MedicalFeeServiceFee.ApproveAdjustment`, `MedicalFeeSharingAgreement.Approve`, `MedicalFeeUnresolvedService.Waive` |
| Admin Master Medical Fee | `MedicalFeeRole.View` + `Manage` |
| Staf Finance | `MedicalFeeHandoff.View` + `Acknowledge` |
| Tenaga medis (penerima) | `MedicalFeeServiceFee.View` — **dibatasi pada dirinya sendiri**, lihat bagian 5 |

Perhatikan bahwa Staf memegang `Calculate` dan `RequestAdjustment` tetapi **tidak** `Verify`;
Supervisor memegang `Verify` tetapi **tidak** `Approve`; Manajer memegang `Approve` tetapi
**tidak** `Calculate` maupun `RequestAdjustment`. Tidak satu pun peran memegang pasangan yang
dilarang di bagian 3.

## 5. Pembatasan data untuk tenaga medis

Tenaga medis yang melihat jasanya sendiri MUST hanya melihat barisnya sendiri. Ini
**pembatasan data**, bukan sekadar pembatasan endpoint — `MedicalFeeServiceFee.View` yang
dipegang tenaga medis menyaring `PayeeReferenceId` terhadap identitas pengguna yang sedang
masuk, di lapis service.

| Yang boleh dilihat | Yang tidak |
|---|---|
| Hasil jasa dirinya sendiri | Hasil jasa orang lain |
| Rincian per layanan miliknya | Rekap periode seluruh penerima |
| Kesepakatan tarif dirinya sendiri | Kesepakatan tarif orang lain |
| Riwayat koreksi pada jasanya | Daftar layanan belum terhitung milik seluruh rumah sakit |

Tanpa penyaringan di service, satu izin `View` akan membuka penghasilan ±220 orang kepada siapa
pun yang memegangnya. Itu risiko privasi yang nyata, bukan teoretis.

## 6. Audit

Seluruh entity mewarisi jejak audit `IdentityModel` (`CreateBy`, `UpdateBy`, `DeleteBy`,
`CancelBy` beserta waktunya). Di atas itu, peristiwa berikut MUST tercatat tersendiri:

| Peristiwa | Yang dicatat | Mengapa |
|---|---|---|
| Perhitungan periode dijalankan | Pelaku, waktu, jumlah penerima, total kotor, jumlah belum terhitung | Menjelaskan mengapa angka berubah antar perhitungan |
| Periode diverifikasi, disetujui, ditutup | Pelaku, waktu, status sebelum dan sesudah | Rantai persetujuan |
| Periode dibuka kembali | Pelaku, waktu, alasan | Pembukaan kembali menghapus rincian — MUST dapat dipertanggungjawabkan |
| Koreksi diajukan, disetujui, ditolak | Pengaju, penyetuju, arah, nilai, alasan | Perubahan nilai uang setelah perhitungan |
| Baris tarif digantikan | Pelaku, persentase lama dan baru, masa berlaku, alasan | Menjelaskan perubahan tarif antar periode |
| Kesepakatan tarif diaktifkan atau dihentikan | Pelaku, waktu, kontrak yang ditunjuk | Kelayakan seseorang menerima jasa |
| Layanan belum terhitung dikesampingkan | Pelaku, waktu, catatan | Jasa yang memang tidak akan terbit |
| Penyerahan ke Finance dibuat, di-ACK, gagal, dikirim ulang | Pelaku, `HandoffKey`, `CorrelationId` | Penelusuran lintas modul |
| Akses ke hasil jasa orang lain | Pelaku, siapa yang dilihat | Data penghasilan orang; aksesnya pantas terlihat |

Baris terakhir sengaja dimasukkan. Modul ini memegang data penghasilan ±220 orang, dan siapa
melihat penghasilan siapa adalah pertanyaan yang cepat atau lambat akan ditanyakan.

## 7. Yang sengaja tidak diberi izin

| Kemampuan | Mengapa tidak ada izinnya |
|---|---|
| Menghapus hasil jasa | Hanya lahir dan hilang lewat perhitungan periode |
| Mengubah nilai hasil jasa langsung | Hanya lewat perhitungan ulang atau koreksi berjenjang |
| Menghapus layanan belum terhitung | Menghapus berarti menyembunyikan (`MF-DEC-018`) |
| Membuat penyerahan ke Finance manual | Hanya lahir dari persetujuan |
| Mengubah `HandoffKey` | Merusak idempotensi di sisi Finance |
| Membuka kembali periode `Closed` | Tidak ada jalan kembali; koreksi lewat `MdfServiceFeeAdjustment` |
