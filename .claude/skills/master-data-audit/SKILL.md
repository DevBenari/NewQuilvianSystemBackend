---
name: master-data-audit
description: Audit master data backend terhadap kebutuhan frontend. Bandingkan kontrak baku master data frontend dengan controller/DTO backend, lalu tulis register kekurangan di docs/hamzah/task/. Pakai saat user minta "cek kekurangan backend", "audit master data", "apa yang belum didukung backend", atau menyebut satu entitas/area master data HR untuk diperiksa. Skill ini TIDAK mengubah kode.
---

# Audit Master Data — Cari Kekurangan Backend

Bandingkan **apa yang dibutuhkan frontend** dengan **apa yang disediakan backend**, lalu
tulis hasilnya sebagai register kekurangan yang siap dipecah jadi tugas.

Bedanya dengan skill `master-data-set`: skill ini **memeriksa dan melapor**, skill itu
**mengerjakan** hasil laporannya.

> ⛔ **Skill ini tidak mengubah satu baris kode pun.** Yang dihasilkan hanya satu dokumen
> register. Kalau di tengah audit tergoda memperbaiki sesuatu — jangan. Catat sebagai
> temuan, biarkan `master-data-set` yang mengerjakan.

---

## Masukan

Salah satu dari:

- **Satu entitas** — `benefit-type`, `leave-policy`, "Jenis Tunjangan"
- **Satu area** — `PayrollAndBenefit`, `Organization`, `LeaveAndOvertime`
- **Semua** — audit menyeluruh master data HR

Kalau user hanya menyebut label Bahasa Indonesia, cari route-nya di
`QuilvianFrontEnd/src/utils/menu-sidebar/menu-items.jsx`.

Kalau masukannya tidak jelas, **tanya dulu** — audit menyeluruh 60+ entitas sangat berbeda
biayanya dari audit satu entitas.

---

## Aturan yang mengikat

Baca sebelum menilai apa pun:

| Dokumen | Isi |
|---|---|
| `.claude/rules/master-data-contract.md` | Kontrak backend — **ini yang dinilai** |
| `.claude/rules/git-read-only.md` | Batas pekerjaan Claude |
| `QuilvianFrontEnd/.claude/rules/rules-master-data.md` | Kontrak UI asli, acuan tertinggi |
| `QuilvianFrontEnd/.claude/rules/no-uuid-display.md` | Sumber kewajiban field `*Name` |

---

## Langkah

### 1. Tentukan cakupan dan kumpulkan daftar entitas

```bash
# Semua controller master data HR di backend
rg -l "class .*Controller" Areas/Corporate/HumanResource/MasterData/ \
   Repositories/Configurations/Corporate/HumanResource/MasterData/

# Modul master data yang ada di frontend
ls ../QuilvianFrontEnd/src/lib/constants/hr/master-data/
```

> ⚠️ Empat controller EmployeeRelation (`violationtypes`, `sanctiontypes`, `actiontypes`,
> `casetypes`) letaknya **ganjil** — di `Repositories/Configurations/`, bukan `Areas/`.
> Rutenya tetap jalan karena ASP.NET memindai atribut `[Route]`. Jangan sampai terlewat
> dari penyisiran hanya karena foldernya beda.

Cocokkan dua arah, keduanya adalah temuan:

- Modul frontend yang **tidak punya** route backend → endpoint hilang
- Model + `DbSet` yang **tidak punya** controller → master tidak bisa dikelola

### 2. Periksa tiap entitas terhadap kontrak

Untuk setiap entitas, baca controller + DTO-nya, lalu isi tabel ini:

| Butir | Cara memeriksa |
|---|---|
| Filter tanggal | Action list punya `[FromQuery] DateTime? startDate`? |
| `customPeriods` di metadata | `GET /filters/metadata` mengisi `CustomPeriods`? |
| `defaultFilter` | Memuat `StartDate`, `EndDate`, `CustomPeriod`? |
| Proyeksi `/options` | Baca `.Select(...)`-nya — apakah mengisi kode + nama, atau hanya `Id`? |
| `CreateByName` | DTO list punya `string? CreateByName`, bukan hanya `Guid? CreateBy`? |
| `UpdateByName` | DTO detail punya `string? UpdateByName`? |
| Endpoint status | `PATCH /{id}/status`, bukan `/activate` + `/deactivate`? |
| Kode auto-generate | `POST` memanggil `GenerateCodeAsync`, bukan menerima kode dari user? |
| `SortOrder` di `PUT` | Ditimpa tanpa syarat (`= request.SortOrder`)? Itu temuan |
| `GET /summary` | Ada, dan tidak ikut difilter tanggal? |

Perintah penyisiran cepat:

```bash
# Controller yang BELUM menerima filter tanggal
rg -L "FromQuery\] DateTime\? startDate" --glob "**/MasterData/**/*Controller.cs" -l

# Proyeksi options yang hanya mengisi Id
rg -B2 -A6 "new \w+OptionResponse\(\)" --glob "**/MasterData/**/*Controller.cs"

# DTO tanpa CreateByName
rg -L "CreateByName" --glob "**/MasterData/**/*Dtos.cs" -l

# SortOrder ditimpa tanpa syarat
rg -n "SortOrder = request\.SortOrder;" --glob "**/MasterData/**/*Controller.cs"
```

### 3. Verifikasi sisi frontend — jangan percaya deskripsi

Sebuah kekurangan baru sah disebut kekurangan backend kalau frontend memang **tidak bisa**
menyelesaikannya sendiri. Cek dua arah:

| Klaim | Cara membuktikan |
|---|---|
| "Filter tanggal tidak jalan" | Cek `unsupportedFilterKeys` di `<entity>-constants.jsx` frontend |
| "Select relasi kosong" | Cek apakah resource-nya terdaftar di `src/lib/hooks/select/hr/hr-select-resources.js`. Select kosong sering karena frontend menunggu `filters/metadata`, **bukan** karena backend kurang |
| "Endpoint tidak ada" | `rg` route-nya di backend. Sering ada tapi namanya beda |
| "Kolom Dibuat Oleh kosong" | Cek DTO backend — kalau `CreateByName` memang tidak ada, itu temuan sah |

> **Jebakan paling sering:** field relasi yang "terpaksa jadi input UUID karena backend
> belum siap". Verifikasi ulang ke controller — sering endpointnya sudah ada dan tidak
> pernah dipanggil frontend. Itu temuan **frontend**, bukan backend.

### 4. Nilai bobotnya sebelum menulis

Tidak semua kekurangan layak jadi tugas. Untuk tiap temuan, tentukan:

| Pertanyaan | Kalau jawabannya... |
|---|---|
| Bisa diselesaikan di frontend tanpa memperburuk kode? | Ya → **bukan** temuan backend |
| Memblokir halaman frontend jalan? | Tidak → tetap dicatat, tapi prioritas lebih rendah |
| Ada pola yang sudah terbukti di repo? | Ya → sifatnya repetitif, biaya rendah |
| Butuh migration? | Ya → prioritas turun, dampaknya lebih besar |

Bedakan tegas antara **"fungsionalitas hilang untuk pengguna"** dan **"halaman tidak bisa
jalan"**. Frontend biasanya sudah menanganinya dengan fallback yang jujur (filter tidak
dikirim, kolom `-`) — jadi sebagian besar temuan **tidak memblokir**. Katakan itu apa adanya.

### 5. Tulis register

Simpan di `docs/hamzah/task/<nama-topik>.md`, kebab-case dan deskriptif.
Satu audit = satu halaman. Kalau topiknya sama dengan halaman yang sudah ada,
**perbarui halaman itu**, jangan bikin duplikat.

```markdown
# <Cakupan> — Daftar Kebutuhan Frontend yang Belum Didukung Backend

| | |
|---|---|
| Tanggal | YYYY-MM-DD |
| Branch | `MHamzah` |
| Pemicu | dari mana kebutuhan ini datang |
| Migration | ada / tidak ada |
| Breaking change | ya / tidak |

Dokumen ini **bukan laporan perubahan**, melainkan **register kebutuhan**.
Tidak ada file kode yang disentuh saat dokumen ini dibuat.

## Ringkasan

| ID | Kebutuhan | Objek terdampak | Dampak kalau dibiarkan | Status |
|---|---|---|---|---|
| **GAP-1** | ... | N controller | ... | ⬜ Belum dikerjakan |

### Cara dokumen ini diverifikasi
Sebutkan bagaimana tiap angka dihitung, dan angka lama mana yang ternyata usang.

## GAP-N — <judul>
### Masalahnya
### Pola acuan — sudah ada di repo, jangan bikin baru
### Kontrak parameter / field
### Daftar objek terdampak
### Definition of Done

## Yang sengaja TIDAK diminta berubah
Tabel: hal → status → alasan. Ini mencegah pekerjaan mubazir.

## Usulan pemecahan tugas
| # | Tugas | Scope | Status | Ketergantungan |

## Pekerjaan yang sudah jalan
| Tugas | Commit | Laporan | Sudah di `origin/MHamzah` |

## Status verifikasi
| Pemeriksaan | Hasil |
```

Aturan isi:

- **Setiap angka dihitung ulang dari kode**, bukan disalin dari dokumen lama. Kalau angka
  di dokumen sebelumnya berbeda, sebutkan bahwa angka itu usang dan kenapa.
- Sebutkan **pola acuan yang sudah ada di repo** untuk tiap GAP, lengkap dengan path dan
  nomor baris. Tugas turunannya harus meniru, bukan mengarang implementasi baru.
- Bagian **"Yang sengaja TIDAK diminta berubah"** wajib ada. Tanpa itu, pembaca berikutnya
  akan mengerjakan hal yang sudah sengaja dilewati.
- Urutkan usulan tugas dari **rasio manfaat/usaha tertinggi**, dan sebutkan tugas mana yang
  layak digabung karena menyentuh file yang sama.
- Bagian **Status verifikasi** tidak boleh memuat klaim yang belum benar-benar dijalankan.

### 6. Laporkan ke user

```markdown
## Audit <cakupan>

### Temuan
| ID | Kebutuhan | Objek terdampak | Sifat pekerjaan |

### Yang sudah benar
Sebutkan singkat — supaya user tahu itu sudah diperiksa, bukan terlewat.

### Yang sengaja dilewati
<hal> → <alasan>

### Register
`docs/hamzah/task/<topik>.md` — <n> GAP, dipecah jadi <m> tugas

### Rekomendasi urutan
T1 dulu karena <alasan>
```

Tutup dengan menawarkan menjalankan `master-data-set` untuk tugas pertama — jangan langsung
mengerjakannya dalam sesi audit yang sama.

---

## Setelah audit

Register ini **belum masuk git** — Claude tidak menjalankan perintah git yang mengubah
apa pun (`.claude/rules/git-read-only.md`). Sajikan perintahnya supaya user tinggal
menyalin:

```bash
git -C QuilvianBackend add docs/hamzah/task/<topik>.md
git -C QuilvianBackend commit -m "docs: register kebutuhan <cakupan>"
git -C QuilvianBackend push origin MHamzah
```
