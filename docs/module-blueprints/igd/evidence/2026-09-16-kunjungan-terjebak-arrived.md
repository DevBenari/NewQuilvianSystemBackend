# Kunjungan IGD Terjebak pada Status `Arrived`

| Field | Nilai |
| --- | --- |
| Tanggal | 16 September 2026 |
| Jenis | Evidence temuan runtime + audit source. **Bukan** implementasi, **bukan** perubahan source |
| Skill | `plan-module-delivery` |
| Blueprint | `IGD-BP-001` revision `6` (`draft`); kontrak state `0.4.0` bagian 1 `approved` lewat `IGD-DEC-093` |
| Backend | `NewQuilvianSystemBackend` branch `rizkiG` `27351517` |
| Frontend | `QuilvianSystemFrontendDev` branch `RizkiV2` `2d95904ed` + working tree (pekerjaan `FE-IGD-028` belum di-commit) |
| Pelapor | Product/Domain Owner IGD, dari pemakaian layar triage |
| Batasan | Tanpa coding, migration, query basis data, build, test, commit |

---

## A. Kesimpulan eksekutif

**Modul IGD tidak dapat dipakai untuk pasien baru.** Perawat tidak pernah bisa menyimpan
pemeriksaan triage, karena kunjungan IGD lahir dengan status `Arrived` dan **tidak ada satu pun
jalan keluar dari status itu yang tersedia di layar**.

Ini bukan cacat pada form triage, dan bukan pula kesalahan penegakan aturan di backend.
Penolakannya justru benar menurut kontrak yang sudah disetujui. Yang hilang adalah **langkah
perpindahan status** — sebuah lubang di alur, bukan sebuah bug.

Kontrak state `0.4.0` bagian 1 (`approved`, `IGD-DEC-093`) memberi `Arrived` tiga jalan keluar
yang sah. Ketiganya sama-sama tidak punya pemanggil:

| Dari `Arrived` ke | Sah menurut kontrak | Tersedia di layar |
| --- | :-: | --- |
| `WaitingForTriage` | ✓ | **Tidak ada** |
| `InTreatment` | ✓ | **Tidak ada** |
| `Cancelled` | ✓ | **Tidak ada** |

Akibatnya dua pasien yang sudah terdaftar pada basis data dev tertahan sebagai "Pasien tiba"
tanpa cara apa pun untuk maju.

---

## B. `IGD-EV-131` — gejala yang terlihat perawat

Dicatat dari pemakaian layar oleh Product/Domain Owner, 16 September 2026.

Perawat membuka **Triage IGD**, memilih pasien `BAGUS SETIAWAN` (No. RM `00-00-00-09`,
encounter `IGD-260831043204-B9E1AF…`, tanggal datang 31 Agustus 2026 11.31), mengisi seluruh
pengkajian triage — keluhan utama, riwayat, tanda vital, sepuluh indikator terpilih, kesimpulan
pengkajian — lalu menekan **Simpan Pemeriksaan**.

Hasilnya:

| Yang tampil | Isi |
| --- | --- |
| Spanduk galat di layar | *"Status kunjungan tidak dapat berubah dari Arrived ke Triaged."* |
| Console peramban | `Failed to load resource: the server responded with a status of 409` pada `:7184/api/v1/health-…emergency-triages` |
| Hasil triage yang sudah dihitung layar | `MERAH – PRIORITAS 1`, 10 indikator terpilih — **hilang**, karena penyimpanannya ditolak |
| Status kunjungan sesudahnya | Tetap `Arrived` |

Daftar pasien triage memperlihatkan **dua** pasien, keduanya berlabel **"Pasien tiba"**:
`BAGUS SETIAWAN` (31 Agustus 2026) dan `NABILA PUTRI MAHARANI` (28 Agustus 2026). Tidak satu pun
pernah berpindah status sejak didaftarkan.

**Pesan galatnya sendiri sudah benar.** `FE-IGD-012` menuntut penolakan backend tampil apa
adanya, dan itulah yang terjadi. Yang salah bukan pesannya, melainkan kenyataan bahwa perawat
tidak diberi cara untuk memenuhinya.

---

## C. `IGD-EV-132` — kunjungan lahir `Arrived` dari dua tempat

| Tempat | Baris | Isi |
| --- | --- | --- |
| `Models/EmgVisit.cs` | 59 | `VisitStatus` default `EmergencyVisitStatus.Arrived` |
| `DTOs/EmergencyVisitDtos.cs` | 100 | `CreateEmergencyVisitRequest.VisitStatus` default `EmergencyVisitStatus.Arrived` |
| Frontend `emergency-registration.utils.js` | 1174-1177 | `visitStatus: toSafeNumber(context.visitStatus, EMERGENCY_VISIT_STATUS.ARRIVED)` |

Baris frontend itu penting: layar pendaftaran **tidak** membiarkan backend memilih default, ia
mengirim `1` secara eksplisit. Jadi mengubah default backend saja tidak akan mengubah apa pun.

Yang membuatnya janggal, payload yang sama juga mengirim `registrationStatus: REGISTERED` dan
`registrationCompletedAt` berisi waktu saat itu. Satu penyimpanan menyatakan dua hal yang
bertentangan: pendaftarannya **sudah selesai**, tetapi kunjungannya **baru tiba**.

---

## D. `IGD-EV-133` — nol penulis `WaitingForTriage` di seluruh backend

Diperiksa dengan tiga pencarian terpisah pada `27351517`, seluruh folder `Areas/`, di luar
`Migrations/`:

| Pencarian | Hasil |
| --- | --- |
| Penulisan langsung `VisitStatus = EmergencyVisitStatus.…` | **nol** |
| Pemanggil `TryApplyVisitStatus(...)` | **empat**, targetnya hanya `Triaged` (dua kali), `InTreatment`, `Disposed`, `Completed` |
| Penyebutan `WaitingForTriage` di luar `CanTransition` dan komentar | **nol** |

Empat pemanggil penjaga transisi itu:

| Berkas | Baris | Target |
| --- | --- | --- |
| `EmergencyTriageController.cs` | 305 dan 455 | `Triaged` |
| `EmergencyResuscitationController.cs` | 301 | `InTreatment` |
| `EmergencyDispositionController.cs` | 338 | `Disposed` |
| `EmergencyVisitController.cs` | 490 | `Completed` |

Artinya `WaitingForTriage` hanya dapat dicapai lewat satu pintu: endpoint status umum
`PATCH /{id}/visit-status`, yang menerima target apa pun yang lolos `CanTransition`.

**Penegakannya sendiri sudah benar** — `EmergencyVisitService.CanTransition` baris 399-403 cocok
persis dengan tabel kontrak bagian 1, dan `BE-IGD-018` sudah menutup tujuh titik tulis liar.
Evidence ini **tidak** mengusulkan pelonggaran aturan itu.

---

## E. `IGD-EV-134` — nol pemanggil `visit-status` di frontend

Diperiksa pada `2d95904ed` + working tree, seluruh `src/`:

| Pencarian | Hasil |
| --- | --- |
| Pemanggilan route `visit-status` | **nol** |
| `WaitingForTriage` / `WAITING_FOR_TRIAGE` di luar berkas konstanta | **nol** |
| Pemakaian `EMERGENCY_VISIT_STATUS.*` di luar berkas konstanta | tiga, seluruhnya **membaca** — default payload pendaftaran dan dua pemeriksaan label |
| Berkas yang menyebut resusitasi | **nol** |

Konstantanya lengkap dan benar — `EMERGENCY_VISIT_STATUS`, labelnya ("Menunggu triage"), dan
warna badge-nya sudah ada di `emergency-registration.constants.js` baris 260-300. Yang tidak
pernah ditulis adalah **aksinya**.

Daftar triage hanya menyediakan satu tombol pada kolom AKSI
(`emergency-triage-patient-table.jsx` baris 86-103): `"Lihat Riwayat"` bila status kunjungan ≥ 3,
selain itu `"Isi Triage"`. Tombol itu hanya berpindah layar; ia tidak pernah menyentuh status.

Baris ini juga menegaskan ulang gap yang sudah tercatat 15 September 2026 pada
`frontend-roadmap.md` bagian "Gap yang dicatat tanpa ID task": `EmergencyResuscitationController`
nol pemakai. Karena resusitasi adalah satu-satunya jalur lain yang mendorong kunjungan ke
`InTreatment`, ketiadaannya menutup jalan keluar terakhir.

---

## F. Akibat pada proses bisnis

Alur yang **diharapkan** berjalan di IGD:

```text
pasien datang -> didaftarkan -> masuk antrean triage -> ditriage -> ditangani -> disposisi -> selesai
```

Alur yang **benar-benar** terjadi hari ini:

```text
pasien datang -> didaftarkan -> [Arrived] -> buntu
```

| Peran | Yang ingin dikerjakan | Yang terjadi |
| --- | --- | --- |
| Petugas pendaftaran | Mendaftarkan pasien IGD | Berhasil. Kunjungan tersimpan `Arrived` |
| Perawat triage | Menyimpan pengkajian triage | **Ditolak `409`**. Pengkajian hilang |
| Perawat triage | Menangani pasien kritis lebih dulu | **Tidak ada tombolnya** |
| Dokter | Melanjutkan ke penanganan, observasi, disposisi | Tidak pernah tercapai — seluruh jalur sesudah triage ikut mati |

Dampaknya melebar ke belakang: seluruh kemampuan yang sudah dibangun `MVP-1` sampai `MVP-5` —
pengkajian, observasi, kepergian, serah terima, disposisi — tidak dapat dijangkau lewat layar
untuk pasien baru, karena pintu pertamanya tertutup.

---

## G. Endpoint yang dibutuhkan sudah ada dan berjalan

Tidak ada endpoint baru yang perlu dibuat. Yang hilang murni pemanggilnya di frontend.

```yaml
PATCH /api/v1/health-services/emergency-installation-management/emergency-visits/{id}/visit-status
summary: Mengubah status kunjungan IGD
security:
  - bearerAuth: []
permission: EmergencyVisit + Update
parameters:
  - name: id
    in: path
    required: true
    schema: { type: string, format: uuid }
requestBody:
  required: true
  content:
    application/json:
      schema:
        type: object
        properties:
          visitStatus:
            type: integer
            description: >
              1 Arrived, 2 WaitingForTriage, 3 Triaged, 4 InTreatment,
              5 UnderObservation, 6 AwaitingDisposition, 7 Disposed,
              8 Cancelled, 9 Completed
            example: 4
          notes:
            type: string
            nullable: true
            description: Catatan bebas; tersimpan pada EmgVisit.Notes
responses:
  "200":
    description: Status kunjungan IGD berhasil diubah
  "400":
    description: Transisi tidak diperbolehkan CanTransition
  "404":
    description: Data kunjungan IGD tidak ditemukan
  "409":
    description: Target Completed ditolak; penyelesaian kunjungan hanya lewat PATCH /{id}/complete
```

Sumber: `EmergencyVisitController.cs` baris 403-448. Perilaku sampingan yang sudah benar dan
tidak perlu ditiru di frontend: saat target `InTreatment`, backend mengisi sendiri
`TreatmentStartedAt` bila masih kosong, lalu mencatat jejak audit lewat `_loggerService`.

---

## H. Temuan sampingan

### `IGD-EV-135` — master level triage di basis data dev tidak sama dengan seeder

| Sumber | Isi |
| --- | --- |
| Layar (basis data dev) | **4 baris**: `MERAH – PRIORITAS 1`, `KUNING – PRIORITAS 2`, `HIJAU – PRIORITAS 3`, `HITAM – MENINGGAL / TIDAK ADA TANDA KEHIDUPAN – PRIORITAS 4` |
| `EmergencyMasterDataSeeder.cs` baris 79-89 | **6 baris**: `L1` Resusitasi, `L2` Emergensi, `L3` Urgen, `L4` Semi-urgen, `L5` Tidak gawat darurat, `L0` Meninggal saat tiba |

Kata "Prioritas" tidak muncul di source mana pun, jadi baris yang dipakai jelas dimasukkan
manual, bukan hasil seeder. Akibatnya nilai `AllowsTreatmentBeforeRegistration` pada baris nyata
**belum diketahui**. Pada seeder, flag itu `true` hanya untuk `L1` dan `L2`, dan ia menentukan
`EmgTriage.ImmediateCareAllowed` serta `EmgVisit.IsImmediateCareAllowed`
(`EmergencyTriageController.cs` baris 261 dan 315).

Perlu pemeriksaan basis data oleh pemilik, bukan keputusan.

### `IGD-EV-136` — kode galat transisi ilegal tidak seragam

| Jalur | Transisi ilegal dijawab |
| --- | --- |
| `PATCH /{id}/visit-status` | **`400`** Bad Request |
| Penyelesaian triage | **`409`** Conflict |

Keduanya memakai penjaga yang sama. Perbedaan kodenya tidak menghalangi pekerjaan ini —
normalisasi galat frontend membaca `response.data.message` untuk keduanya — tetapi dicatat supaya
penanganan galat tidak dibangun di atas asumsi "transisi ilegal selalu `409`".

---

## I. Usulan yang lahir dari bukti ini

Bukan keputusan. Diajukan kepada Product/Domain Owner pada tanggal yang sama.

| Usulan | Bentuk |
| --- | --- |
| Pendaftaran IGD menutup dengan `WaitingForTriage`, bukan `Arrived` | Keputusan domain → `IGD-DEC-127` |
| Penanganan cepat dijalankan lewat aksi status pada daftar triage | Keputusan domain → `IGD-DEC-128` |
| Satu task frontend untuk masing-masing | `FE-IGD-029`, `FE-IGD-030` |

Yang **tidak** diusulkan, dan alasannya:

| Ditolak | Alasan |
| --- | --- |
| Membuat backend meloncatkan `Arrived` → `Triaged` | Rumusan itu sudah pernah ditolak Product/Domain Owner dan diganti `IGD-DEC-104`; ia menyembunyikan transisi ilegal menjadi keberhasilan diam-diam |
| Melonggarkan `CanTransition` | Cacatnya bukan pada matriks. Matriksnya sudah benar dan sudah disetujui |
| Membangun layar resusitasi untuk membuka jalur cepat | Jauh lebih besar dari kebutuhannya, dan owner sudah memutuskan 15 September 2026 bahwa layar resusitasi tidak diberi ID task |
