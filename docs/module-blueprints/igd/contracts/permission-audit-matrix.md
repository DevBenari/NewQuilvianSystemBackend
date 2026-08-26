# Matriks Hak Akses dan Audit — Modul IGD

| Field | Nilai |
| --- | --- |
| `contract_version` | `0.3.0` |
| Status | `draft` |
| Owner | Security/Privacy owner — **belum ditunjuk**; Product/Domain Owner IGD sebagai pemegang sementara |
| `approved_by` / `approved_at` | — / — |
| Versi sebelumnya | `0.2.0` |

---

## 1. Dua lapis kewenangan

`IGD-DEC-058` dan `IGD-DEC-086` menetapkan dua lapis yang **saling melengkapi dan tidak
saling menggantikan**:

| Lapis | Menjawab | Ditegakkan oleh |
| --- | --- | --- |
| Kemampuan | "Boleh melakukan tindakan jenis ini?" | `[AccessPermission("Resource", "Action")]` pada endpoint |
| Unit | "Boleh melakukannya pada unit ini?" | `EmergencyUnitAuthorityService` di dalam service IGD |

> Memiliki penugasan unit **tidak** dengan sendirinya memberi kemampuan klinis, dan memiliki
> kemampuan **tidak** melewati batas penugasan unit.

---

## 2. Resource dan aksi

| Resource | `Read` | `Create` | `Update` | `Delete` | `Approve` |
| --- | :-: | :-: | :-: | :-: | :-: |
| `EmergencyVisit` | ✓ | ✓ | ✓ | ✓ | — |
| `EmergencyTriage` | ✓ | ✓ | ✓ | ✓ | — |
| `EmergencyTriageDetail` | ✓ | ✓ | ✓ | ✓ | — |
| `EmergencyObservation` | ✓ | ✓ | ✓ | ✓ | — |
| `EmergencyObservationDetail` | ✓ | ✓ | ✓ | ✓ | — |
| `EmergencyResuscitation` | ✓ | ✓ | ✓ | ✓ | — |
| `EmergencyProcedureDetail` | ✓ | ✓ | ✓ | ✓ | — |
| `EmergencyDisposition` | ✓ | ✓ | ✓ | ✓ | — |
| `EmergencyDeparture` | ✓ | ✓ | ✓ | ✓ | **✓ baru** |
| `EmergencyDoctorAssignment` | ✓ | ✓ | ✓ | — | — |

`EmergencyDeparture : Approve` adalah aksi baru, dipakai **hanya** untuk membalik kejadian
yang salah pasien atau salah unit. Ia sengaja dipisahkan dari `Update` supaya kewenangan
membalik dapat diberikan kepada orang yang berbeda dari yang mencatat.

Nama resource lama `EmergencyTransfer` **dihapus** setelah masa peralihan. Selama peralihan,
kebijakan yang menunjuk `EmergencyTransfer` **wajib** dipetakan ke `EmergencyDeparture`;
melewatkan pemetaan ini membuat seluruh petugas kehilangan akses tanpa pesan yang menjelaskan.

---

## 3. Kewenangan unit per tindakan

| Tindakan | Wajib berwenang atas | Sebab |
| --- | --- | --- |
| Mencatat kedatangan pasien | Unit **tujuan** | `IGD-DEC-064` — hanya petugas unit penerima yang menjadi bukti kedatangan |
| Menerima serah terima | Unit **tujuan** | `IGD-DEC-061` |
| Menolak serah terima | Unit **tujuan** | `IGD-DEC-061` |
| Membuat catatan kepergian | Unit **asal** | Pengirim |
| Mencatat keberangkatan | Unit **asal** | `IGD-DEC-072` |
| Membatalkan kepergian | Unit **asal** | |
| Membalik kejadian | Unit **asal atau tujuan**, ditambah `Approve` | `IGD-DEC-066` |
| Menetapkan dan mengalihkan dokter | Unit **asal** | |
| Membaca daftar pantau | Unit mana pun tempat pengguna bertugas | Daftar disaring menurut penugasannya |

---

## 4. Pemisahan tugas

| Aturan | Ditegakkan di | Keputusan |
| --- | --- | --- |
| Penyetuju pembalikan **wajib berbeda** dari pengaju | Service, dibandingkan `RecordedByUserId` dengan `ApprovedByUserId` | `IGD-DEC-066` |
| Pengirim serah terima tidak dapat menerima serah terimanya sendiri | Service, lewat kewenangan unit tujuan | `IGD-DEC-061` |
| Pencatat kedatangan bukan pengantar dari IGD | Service, lewat kewenangan unit tujuan | `IGD-DEC-064` |

Cacat `BE-IGD-016` — kolom penerima terisi oleh pengajunya sendiri — **tidak boleh terulang**.
Pada `0.3.0` kolom penerima hanya terisi oleh tindakan penerimaan, bukan oleh pembuatan.

---

## 5. Yang dicatat pada jejak audit

### 5.1 Catatan klinis — tambah-saja

`IGD-DEC-080` menetapkan catatan klinis tidak diubah di tempat. Yang dicatat:

| Yang disimpan | Di mana |
| --- | --- |
| Nilai asli beserta seluruh kolom klinisnya | Baris lama, `IsEffective = false` |
| Nilai hasil koreksi | Baris baru, `IsEffective = true` |
| Penunjuk ke baris yang dikoreksi | `Amends…Id` |
| Alasan koreksi | `AmendmentReason` |
| Pelaku dan waktu | `CreateBy`, `CreateDateTime` pada baris baru |

Berlaku untuk pengkajian, tanda vital, catatan perkembangan, penilaian triase, dan kejadian
kepergian.

### 5.2 Kejadian kepergian

Setiap kejadian menyimpan: jenis, waktu sebenarnya, waktu server, pelaku, unit pelaku, alasan,
penunjuk kejadian yang digantikan, dan penyetuju bila berupa pembalikan.

### 5.3 Apa yang **tidak** boleh masuk log

| Larangan | Sebab |
| --- | --- |
| Isi klinis lengkap — SBAR, keluhan, ringkasan pemeriksaan | `IGD-DEC-006` melarang PHI masuk mutation logging |
| Nama pasien dan nomor rekam medis pada berkas log | `IGD-DEC-006` |
| Nama sementara pasien tanpa identitas | Sama |

Yang boleh dicatat pada `LoggerService`: identitas baris, nama aksi, pelaku, dan waktu.

### 5.4 Keterbatasan yang tetap ada

`LoggerService.AuditAsync` menulis ke Serilog, **bukan** ke tabel yang dapat ditelusuri per
baris data. Untuk tabel non-klinis, jejak audit tetap hanya `IdentityModel` — pelaku terakhir
saja. `IGD-DEC-080` sengaja **tidak** memperluas ini ke tabel non-klinis.

Akibatnya: pertanyaan "siapa yang mengubah target waktu triase pada master, dan kapan" **tetap
tidak dapat dijawab**. Ini keterbatasan yang disadari, bukan kelalaian.

---

## 6. Gerbang yang belum terpenuhi

| Gerbang | Menunggu | Akibat |
| --- | --- | --- |
| Pemisahan kewenangan SuperAdmin | Security/Privacy owner | Boleh dibangun dan diuji; **tidak boleh diaktifkan di produksi** |
| Break-glass akses darurat | Security/Privacy owner, `IGD-OQ-037` | Wajib tersedia sebelum pemisahan SuperAdmin diaktifkan |
| Perilaku unit tanpa simpul organisasi | Security/Privacy owner, `IGD-OQ-071` | Penjagaan kewenangan unit **tidak boleh dinyalakan** sebelum diputuskan |
| Pengisian data penugasan unit | Corporate/HR | Sama |
| Kewenangan sementara perawat bantuan | `IGD-OQ-067` | Sama |

Prinsip yang tidak berubah: **gerbang menolak tindakan privileged, integrasi, dan finansial.
Gerbang tidak pernah memblokir pelayanan klinis darurat.**
