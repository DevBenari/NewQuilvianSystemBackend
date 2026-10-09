# Permintaan Persetujuan — CITO dari pemesanan sampai ke pemeriksaan

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-022` |
| `tanggal` | 2026-10-09 |
| `pengaju` | `design-business-module`, menurunkan Amendment Pass putaran 28 |
| `rujukan` | `LAB-DEC-225`..`LAB-DEC-228`; decision log revision 93; `02-backend-architecture.md` bagian 29; `contracts/api-contract.md` bagian 40; `AC-318`..`AC-322` |
| `status` | ✅ **`disetujui`** 2026-10-09 — Yoga Aji Pratama, pemilik modul: *"setuju keenam butir"*; **keenam butir bagian 3 sesuai usulan**, termasuk butir 4 (perbaikan data lama diperluas ke wadah pengganti yang kehilangan *Tandai Cito*) |
| `ditujukan kepada` | Yoga Aji Pratama — pemilik modul Laboratorium |
| `sifat` | Usulan amandemen satu kontrak (`LAB-API-v1` `r45`). Keputusan bisnisnya **sudah** diambil 2026-10-09; yang diminta adalah persetujuan **bentuk kontrak dan cara memperbaiki data lama**. **Kode belum diubah** — implementasi menunggu persetujuan ini |

---

## 1. Satu paragraf untuk yang tidak punya waktu

Dokter dan kiosk bisa memilih CITO untuk suatu pemeriksaan saat memesan, tetapi begitu analis merencanakan wadah,
pemeriksaan itu selalu menjadi "biasa". Akibatnya pasien CITO tidak didahulukan di Daftar Kerja, tidak terpantau
keterlambatannya, dan labelnya tercetak biasa. Saat sampel diambil ulang, tanda CITO yang dipasang dokter susulan
juga hilang. Usulan ini membuat pemeriksaan mewarisi CITO dari permintaannya, membuat wadah pengganti mewarisi
keadaan terakhir pemeriksaan lama, dan memperbaiki pekerjaan yang masih berjalan sekali saat rilis. **Tidak ada
tabel, kolom, endpoint, izin, maupun perubahan layar.**

## 2. Usulan amandemen kontrak

| Kontrak | Revisi usulan | Bagian | Isi |
|---|---|---|---|
| `LAB-API-v1` | `r45` | [40](../contracts/api-contract.md) | Perilaku `urgency` saat rencana wadah dan ambil ulang; perbaikan data lama |

`LAB-STATE-v1`, `LAB-VAL-v1`, `LAB-PERM-v1`, dan `LAB-INT-v1` **tidak berubah**: kesegeraan bukan status, tidak ada
penolakan baru, tidak ada aksi baru, dan tidak ada modul lain yang disentuh.

## 3. Enam butir yang diminta persetujuannya

| No | Butir | Usulan | Bila tidak disetujui |
|---|---|---|---|
| 1 | Penanda pada pemeriksaan yang lahir CITO | `urgency = Cito` dengan `urgencyMarkedAt`/`urgencyMarkedByUserName` **`null`** — konsumen wajib menerima Cito tanpa penanda. Rincian pesanan menampilkan lencana *Cito* dengan waktu `-` | — (sudah diputuskan Q3 putaran 28; butir ini hanya mengunci bentuk kontraknya) |
| 2 | Ambil ulang | Pemeriksaan pengganti menyalin `urgency` **dan** penandanya dari pemeriksaan yang digantikan | Penanda dikosongkan di pengganti; riwayat siapa yang menandai hilang dari pemeriksaan yang dikerjakan |
| 3 | Tambah pemeriksaan manual (`POST /lab-examinations/by-order/{labOrderId}`) | **Tetap `Routine`** — jalur ini tidak bertaut ke permintaan; dokter memakai *Tandai Cito* | Jalur manual ikut mencari permintaan yang cocok — perubahan di luar scope putaran 28 |
| 4 | **Perluasan perbaikan data lama ke cacat kedua** | **Ya** — pemeriksaan di wadah pengganti yang kehilangan CITO dari pemeriksaan yang digantikan (pesanan dan pemeriksaan masih berjalan, pengganti `Routine` berpenanda kosong) ikut dikembalikan ke `Cito` beserta penanda lamanya, dengan satu baris riwayat. Dev 2026-10-09: **0** baris | Hanya permintaan CITO yang diperbaiki (`LAB-DEC-228` apa adanya); pengganti yang kehilangan *Tandai Cito* tetap biasa dan harus ditandai ulang dokter |
| 5 | `version` pemeriksaan yang diperbaiki | **Dinaikkan satu** — layar yang sedang membuka pemeriksaan itu menerima `409` sekali lalu memuat ulang | Tidak dinaikkan; perubahan dapat tertimpa diam-diam oleh penyimpanan yang sedang berlangsung |
| 6 | Cara perbaikan | Migration data `BackfillLabExaminationUrgencyFromOrderedProcedure` — idempoten, riwayat berpenanda `ReasonCode = 'LAB-CONFLICT-019'` dan pelaku sistem (`Guid.Empty`), `Down` hanya membalik baris berpenanda; **hitung kering baca-saja sebelum `database update` di setiap lingkungan** | Skrip SQL manual di luar migration — tanpa jejak `__EFMigrationsHistory` dan tanpa `Down` |

## 4. Cara menjawab

Cukup *"setuju keenam butir"*, atau sebutkan nomor butir yang ingin diubah.
