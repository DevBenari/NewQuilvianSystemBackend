# Desain Domain — Modul Gizi

| Field | Nilai |
|---|---|
| Blueprint ID | `gizi` |
| Revision | `1` |
| Status | `approved` |
| Prefix entity | `Gz` |
| Base URL | `api/v1/health-services/nutrition-management/...` |
| Grup Swagger | `Health Services / Nutrition Management / ...` |
| Dasar | `GIZ-DEC-001` sampai `GIZ-DEC-012` |

Prefix `Gz` dan base URL di atas belum dipakai entity maupun controller mana pun; diperiksa
terhadap berkas kavling registry sebelum ditetapkan.

## Bentuk keseluruhan

```text
TrxPatientEncounter  (milik Registration, dibaca saja)
        |
        v
GzNutritionOrder                        satu per episode rawat inap
  status: Requested -> InProgress -> Closed | Cancelled
        |
        +-- GzNutritionCareRecord       satu per kunjungan ahli gizi
        |     asesmen, diagnosis, intervensi, evaluasi
        |     diet dan kebutuhan energi
        |     recall asupan
        |     -> menunjuk balik ke baris CPPT
        |
        +-- GzNutritionOrderHistory     jejak perubahan status
```

Dua entity utama, satu entity riwayat. Tidak lebih, karena setiap entity tambahan menambah
tempat data bisa menjadi tidak sinkron.

## Entity

### GzNutritionOrder

Permintaan konsultasi gizi untuk satu episode rawat inap. Dibuat dokter penanggung jawab
(`GIZ-DEC-007`), memakai entity sendiri dan bukan `TrxDoctorConsultation` (`GIZ-DEC-001`).

| Kolom | Tipe | Keterangan |
|---|---|---|
| `Id` | `Guid` | |
| `OrderNumber` | `varchar(50)` | Nomor order, unik |
| `PatientId` | `Guid` | Menunjuk `MstPatient` |
| `EncounterId` | `Guid` | Menunjuk `TrxPatientEncounter` |
| `RequesterDoctorId` | `Guid` | Menunjuk `MstDoctor` |
| `AssignedWorkforceId` | `Guid?` | Ahli gizi yang menangani, boleh kosong saat dibuat |
| `Status` | enum | `Requested`, `InProgress`, `Closed`, `Cancelled` |
| `Priority` | enum | `Routine`, `Urgent` |
| `ReasonForReferral` | `varchar(1000)` | Alasan rujukan dari dokter |
| `ScreeningRiskStatus` | enum? | Disalin dari `TrxPatientAssessment` saat order dibuat |
| `ScreeningScore` | `int?` | Disalin bersamaan |
| `RequestedAt` | `timestamptz` | |
| `ClosedAt` | `timestamptz?` | |
| `ClosingNote` | `varchar(2000)?` | Catatan penutup ahli gizi (`GIZ-DEC-008`) |
| `Version` | `int` | Token konkurensi |

**Kenapa hasil skrining disalin, bukan dibaca ulang.** Skrining adalah alasan order ini
lahir. Bila dibaca ulang setiap kali, angkanya berubah ketika perawat memperbarui asesmen,
dan alasan order menjadi tidak lagi cocok dengan apa yang dilihat dokter saat memesannya.
Penyalinan pada saat transaksi seperti ini adalah pengecualian yang sah menurut aturan data
bersama pada registry.

### GzNutritionCareRecord

Satu kunjungan ahli gizi. Berulang selama pasien dirawat (`GIZ-DEC-005`).

| Kolom | Tipe | Keterangan |
|---|---|---|
| `Id` | `Guid` | |
| `NutritionOrderId` | `Guid` | Induknya |
| `VisitSequence` | `int` | Kunjungan ke berapa, mulai dari 1 |
| `VisitAt` | `timestamptz` | |
| `RecordedByWorkforceId` | `Guid` | Ahli gizi yang mencatat |
| `RecordType` | enum | `Initial`, `FollowUp` |
| **Asesmen** | | |
| `Weight` / `Height` / `Bmi` | `decimal?` | Boleh diisi ulang bila diukur saat kunjungan |
| `AssessmentNote` | `varchar(2000)?` | |
| **Diagnosis gizi** | | |
| `NutritionDiagnosisId` | `Guid?` | Menunjuk `MstDiagnosis` bertipe `NUTRITION` (`GIZ-DEC-009`, `GIZ-DEC-011`) |
| `DiagnosisNote` | `varchar(1000)?` | |
| **Intervensi** | | |
| `InterventionNote` | `varchar(2000)?` | |
| `DietPrescription` | `varchar(500)?` | Diet yang ditetapkan |
| `EnergyRequirementKcal` | `int?` | **Diketik ahli gizi, tanpa rumus** (`GIZ-DEC-012`) |
| **Recall asupan** | | |
| `IntakeRecallNote` | `varchar(2000)?` | |
| `IntakePercent` | `int?` | Perkiraan persentase asupan terhadap kebutuhan |
| **Monitoring dan evaluasi** | | |
| `EvaluationNote` | `varchar(2000)?` | |
| **Tautan CPPT** | | |
| `ProgressNoteId` | `Guid?` | Baris CPPT yang dibuat untuk kunjungan ini (`GIZ-DEC-010`) |
| `Version` | `int` | Token konkurensi |

**Kenapa satu entity, bukan lima.** Asesmen, diagnosis, intervensi, recall, dan evaluasi
selalu dicatat bersama dalam satu kunjungan dan tidak pernah berdiri sendiri. Memecahnya
menjadi lima tabel berarti lima baris yang harus dijaga tetap sinkron tanpa manfaat apa pun.

**Kenapa hampir semuanya boleh kosong.** Ahli gizi mengisi bertahap selama kunjungan.
Memaksa semua terisi sekaligus membuat catatan tidak bisa disimpan di tengah pekerjaan, dan
petugas akan mengakalinya dengan mengisi sembarang nilai.

### GzNutritionOrderHistory

Jejak perubahan status order. Mengikuti pola `OprStatusHistory` yang sudah ada.

| Kolom | Tipe |
|---|---|
| `Id`, `NutritionOrderId` | `Guid` |
| `FromStatus`, `ToStatus` | enum |
| `Action` | `varchar(50)` |
| `Reason` | `varchar(1000)?` |
| `ActorUserId` | `Guid` |
| `OccurredAt` | `timestamptz` |
| `Source` | `varchar(100)` — `API:{fingerprint}` untuk idempotensi |
| `CorrelationId` | `varchar(100)` |

## Transisi status

```text
            buat order
                v
          [ Requested ] ---- batal ----> [ Cancelled ]
                |
      kunjungan pertama dicatat
                v
         [ InProgress ] ---- batal ----> [ Cancelled ]
                |
        tutup asuhan gizi
                v
           [ Closed ]
```

| Dari | Ke | Pemicu | Aturan |
|---|---|---|---|
| — | `Requested` | Dokter membuat order | Pasien harus punya encounter yang cocok |
| `Requested` | `InProgress` | Kunjungan pertama disimpan | Otomatis, bukan tombol tersendiri |
| `Requested`, `InProgress` | `Closed` | Ahli gizi menutup asuhan | Wajib ada catatan penutup |
| `Requested`, `InProgress` | `Cancelled` | Dibatalkan | Wajib ada alasan |
| `Closed`, `Cancelled` | — | — | Tidak ada transisi keluar |

Status naik ke `InProgress` **otomatis** saat kunjungan pertama disimpan, bukan lewat tombol
terpisah. Tombol yang tidak menandai kejadian nyata hanya menambah langkah yang gampang lupa
ditekan, dan status pun menjadi berbohong.

## Aturan validasi

| Kode | Aturan |
|---|---|
| `GIZ001` | Encounter harus milik pasien yang sama dan belum dihapus |
| `GIZ002` | Satu episode rawat inap hanya boleh punya satu order aktif |
| `GIZ003` | Alasan rujukan wajib diisi |
| `GIZ004` | Kunjungan hanya boleh dicatat pada order `Requested` atau `InProgress` |
| `GIZ005` | Diagnosis gizi harus bertipe `NUTRITION` bila diisi |
| `GIZ006` | Kebutuhan energi antara 1 sampai 10000 kkal bila diisi |
| `GIZ007` | Persentase asupan antara 0 sampai 100 bila diisi |
| `GIZ008` | Catatan penutup wajib diisi saat menutup asuhan |
| `GIZ009` | Alasan wajib diisi saat membatalkan |
| `GIZ012` | Versi tidak cocok, data sudah diubah pengguna lain |
| `GIZ013` | Idempotency key dipakai dengan isi permintaan berbeda |

## Kontrak API

Base: `api/v1/health-services/nutrition-management`

| Metode | Alamat | Guna |
|---|---|---|
| `GET` | `/orders` | Daftar pemesanan, dengan saring status dan pencarian |
| `GET` | `/orders/{id}` | Detail satu order beserta kunjungannya |
| `POST` | `/orders` | Membuat order konsultasi gizi |
| `PUT` | `/orders/{id}` | Mengubah order yang belum ditutup |
| `POST` | `/orders/{id}/close` | Menutup asuhan gizi |
| `POST` | `/orders/{id}/cancel` | Membatalkan order |
| `GET` | `/orders/{id}/records` | Daftar kunjungan pada satu order |
| `POST` | `/orders/{id}/records` | Mencatat kunjungan baru |
| `PUT` | `/orders/{id}/records/{recordId}` | Mengubah catatan kunjungan |
| `GET` | `/orders/screening-candidates` | Pasien rawat inap berisiko gizi yang belum punya order |

Seluruh perintah yang mengubah data membawa `idempotencyKey` dan `expectedVersion`,
mengikuti pola modul Operasi.

## Hak akses

| Controller | Aksi |
|---|---|
| `NutritionOrder` | `Read`, `Create`, `Update`, `Cancel` |
| `NutritionCareRecord` | `Read`, `Update` |

Izin diberikan lewat `SysAccessPolicy` per Departemen dan Jabatan, bukan per orang.

## Yang sengaja tidak dibuat

| Yang ditolak | Alasan |
|---|---|
| Entity skrining gizi | Sudah ada di `TrxPatientAssessment` |
| Entity kunjungan tersendiri di luar CPPT | `GIZ-DEC-010`: memakai CPPT yang sudah ada |
| Master diagnosis gizi tersendiri | `GIZ-DEC-009`: menumpang `MstDiagnosis` |
| Rumus kebutuhan gizi | `GIZ-DEC-012`: diketik ahli gizi |
| Pemesanan makanan ke dapur | `GIZ-DEC-004`: di luar scope |
